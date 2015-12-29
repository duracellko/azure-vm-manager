/// <reference path="jquery-2.1.4.js" />
/// <reference path="knockout-3.4.0.js" />

(function (window, undefined) {
    var OperationStatus = {
        InProgress: 0,
        Succeeded: 1,
        Failed: 2
    }

    var virtualMachine = function (manager) {
        this._t = this;
        this._manager = manager;
        this.name = ko.observable("");
        this.cloudServiceName = ko.observable("");
        this.operationId = null;
        this.operationStatus = ko.observable(null);
        this.status = ko.observable(null);
        this.statusDisplayName = ko.computed(
            function () {
                var status = this.status();
                var displayName = this._statusDisplayNames[status];
                if (typeof (displayName) == "undefined") {
                    displayName = status;
                }
                
                if (this.operationStatus() == OperationStatus.InProgress) {
                    displayName = "In progress [" + displayName + "]";
                }

                return displayName;
            }, this);
        this.canStartVM = ko.computed(
            function () {
                return (this.operationStatus() == null || this.operationStatus() != OperationStatus.InProgress) &&
                    (this.status() == "StoppedVM" || this.status() == "StoppedDeallocated");
            }, this);
        this.canStopVM = ko.computed(
            function () {
                return (this.operationStatus() == null || this.operationStatus() != OperationStatus.InProgress) &&
                    this.status() == "ReadyRole";
            }, this);
        this.canDownloadRDP = ko.computed(
            function () {
                return (this.operationStatus() == null || this.operationStatus() != OperationStatus.InProgress) &&
                    this.status() == "ReadyRole";
            }, this);
        this.rowClassName = ko.computed(
            function () {
                if (this.operationStatus() == OperationStatus.InProgress) {
                    return "info";
                }
                else if (this.status() == "StoppedVM" || this.status() == "StoppedDeallocated") {
                    return "warning";
                }
                else if (this.status() == "ReadyRole") {
                    return "success";
                }
                else {
                    return "info";
                }
            }, this);
    }

    virtualMachine.prototype = {
		name: null,
		cloudServiceName: null,
		status: null,
        statusDisplayName: null,
		canStartVM: null,
		canStopVM: null,
		canDownloadRDP: null,
		operationId: null,
        operationStatus: null,
		startVM: function () {
		    var t = this._t;
            t._manager.startVM(t);
		},
		stopVM: function () {
            var t = this._t
		    t._manager.stopVM(t);
		},
		downloadRDP: function () {
		    var t = this._t;
		    t._manager.downloadRDP(t);
		},
		updateOperationStatus: function (status) {
		    var t = this._t;
		    t.operationStatus(status);
		    if (status == OperationStatus.InProgress && t._operationTimer == null) {
		        t._operationTimer = window.setTimeout(function () { t._onOperationTimer(); }, 5000);
		    }
		},
		_onOperationTimer: function () {
		    var t = this._t;
		    t._operationTimer = null;
		    if (t.operationStatus() == OperationStatus.InProgress) {
		        t._manager.getOperationStatus(t);
		    }
		},

		rowClassName: null,
        _operationTimer: null,
		_statusDisplayNames: {
		    RoleStateUnknown: "Unknown Role State",
		    CreatingVM: "Creating VM",
		    StartingVM: "Starting VM",
		    CreatingRole: "Creating Role",
		    StartingRole: "Starting Role",
		    ReadyRole: "Running (Role Ready)",
		    BusyRole: "Busy Role",
		    StoppingRole: "Stopping Role",
		    StoppingVM: "Stopping VM",
		    DeletingVM: "Deleting VM",
		    StoppedVM: "Stopped VM",
		    RestartingRole: "Restarting Role",
		    CyclingRole: "Cycling Role",
		    FailedStartingRole: "Staring Role Failed",
		    FailedStartingVM: "Starting VM Failed",
		    UnresponsiveRole: "Unresponsive Role",
		    StoppedDeallocated: "Stopped (Deallocated)"
		}
    }

	var virtualMachinesViewModel = function (manager) {
	    this._t = this;
	    this._manager = manager;
	}

	virtualMachinesViewModel.prototype = {
	    virtualMachines: null,
	    refresh: function () {
	        var t = this._t;
	        t._manager.refresh(true);
	    },
	    showError: function (errorMessage) {
	        if (errorMessage != null && errorMessage != "") {
	            var errorPanel = $("#errorPanel");
	            var errorMessageSpan = $("#errorMessage", errorPanel);
	            errorMessageSpan.text(errorMessage);
	            errorPanel.slideDown();
	        }
	    },
	    hideError: function () {
	        var errorPanel = $("#errorPanel");
	        errorPanel.slideUp();
	    },
	    showInfo: function (message) {
	        if (message != null && message != "") {
	            var infoPanel = $("#infoPanel");
	            var infoMessageSpan = $("#infoMessage", infoPanel);
	            var infoMessage = message;
	            if (message.length > 5 && message.substr(0, 5) == "data-") {
	                infoMessage = infoMessageSpan.attr(message);
	            }
	            infoMessageSpan.text(infoMessage);
	            infoPanel.slideDown();
	        }
	    },
        hideInfo: function () {
	        var infoPanel = $("#infoPanel");
	        infoPanel.slideUp();
	    }
    }

	var azureManager = function (container, serviceUrl) {
		this._t = this;
		this._container = $(container);
		this._serviceUrl = serviceUrl;
		this._virtualMachines = ko.observableArray();
	}

	azureManager.prototype = {
		_virtualMachinesViewModel: null,
		_virtualMachines: null,
		initialize: function () {
			var t = this._t;
			t._virtualMachinesViewModel = new virtualMachinesViewModel(t);
			t._virtualMachinesViewModel.virtualMachines = t._virtualMachines;
			ko.applyBindings(t._virtualMachinesViewModel, $("#azureManagerContainer", t._container).get(0));
		},
		_createAjaxRequest: function (blockUI) {
		    var t = this._t;
		    return {
		        beforeSend: function (jqXHR, settings) {
		            if (blockUI) {
		                $("#errorPanel").hide();
		                $.blockUI({ message: "Processing..." });
		            }
		        },
		        complete: function (jqXHR, textStatus) {
		            if (blockUI) {
		                $.unblockUI();
		            }
		        },
		        error: t._processAjaxError
		    };
		},
		_processAjaxError: function (jqXHR, textStatus, errorThrown) {
		    var errorMessage = "";
		    if (jqXHR.status == 500 && jqXHR.statusText == "AzureManagementError") {
		        var errorXml = null;
		        if (typeof (jqXHR.responseXML) == "undefined" || jqXHR.responseXML == null) {
		            errorXml = $.parseXML(jqXHR.responseText);
		        }
		        else {
		            errorXml = jqXHR.responseXML;
		        }
		        errorMessage = $(errorXml).children("Error").children("Message").text();
		    }
		    else {
		        errorMessage = "Unexpected error occured.";
		    }

		    if (errorMessage != "") {
		        var errorPanel = $("#errorPanel");
		        var errorMessageSpan = $("#errorMessage", errorPanel);
		        errorMessageSpan.text(errorMessage);
		        errorPanel.slideDown();
		    }
		},
		refresh: function (blockUI) {
		    if (typeof (blockUI) == "undefined") {
		        blockUI = true;
		    }

		    var t = this._t;
		    var url = t._serviceUrl + "/VirtualMachines";
		    var r = this._createAjaxRequest(blockUI);
		    r.context = t;
		    r.url = url;
		    r.dataType = "json";
		    r.success = t._onVirtualMachinesLoaded;
		    $.ajax(r);
		},
		_onVirtualMachinesLoaded: function (data) {
		    var t = this._t;

		    // remove non-existing virtual machines
		    t._virtualMachines.removeAll(
                function (item) {
                    for (i = 0; i < data.length; i++) {
                        if (data[i].Name == item.name()) {
                            return false;
                        }
                    }

                    return true;
                });

            // add or update virtual machines
			for (var i = 0; i < data.length; i++) {
			    var vmData = data[i];
			    var virtualMachines = t._virtualMachines();
			    var vm = null;
			    for (var vmIndex = 0; vmIndex < virtualMachines.length; vmIndex++) {
			        if (virtualMachines[vmIndex].name() == vmData.Name) {
			            vm = virtualMachines[vmIndex];
			            break;
			        }
			    }

			    if (vm == null) {
			        vm = new virtualMachine(t);
				    vm.name(vmData.Name);
				    t._virtualMachines.push(vm);
			    }

				vm.cloudServiceName(vmData.CloudServiceName);
				vm.status(vmData.Status);
			}
		},
		getOperationStatus: function (vm) {
		    var t = this._t;
		    var url = t._serviceUrl + "/Operation/" + vm.operationId;
		    var r = this._createAjaxRequest(false);
		    r.context = { manager: t, virtualMachine: vm };
		    r.url = url;
		    r.dataType = "json";
		    r.success = t._onGetOperationStatusEnd;
		    $.ajax(r);
		},
		_onGetOperationStatusEnd: function (data) {
		    var t = this.manager;
		    var vm = this.virtualMachine;
		    vm.updateOperationStatus(data.Status);

		    if (data.Status == OperationStatus.Succeeded) {
		        vm.status("refreshing...");
		        t.refresh(false);
		    }
		    else if (data.Status == OperationStatus.Failed) {
		        var errorMessage = data.ErrorCode + ": " + data.ErrorMessage;
		        t._virtualMachinesViewModel.showError(errorMessage);
		    }
		},
		startVM: function (vm) {
		    var t = this._t;
		    var url = t._serviceUrl + "/StartVM/" + vm.name() + "?cloudService=" + vm.cloudServiceName();
		    var r = this._createAjaxRequest(true);
		    r.context = { manager: t, virtualMachine: vm };
		    r.url = url;
		    r.dataType = "json";
		    r.success = t._onStartVmEnd;
		    $.ajax(r);
		},
		_onStartVmEnd: function (data) {
		    var t = this.manager;
		    var vm = this.virtualMachine;
		    vm.operationId = data;
		    vm.updateOperationStatus(OperationStatus.InProgress);
		    t._virtualMachinesViewModel.showInfo("data-startvm");
		},
		stopVM: function (vm) {
		    var t = this._t;
		    var url = t._serviceUrl + "/ShutdownVM/" + vm.name() + "?cloudService=" + vm.cloudServiceName();
		    var r = this._createAjaxRequest(true);
		    r.context = { manager: t, virtualMachine: vm };
		    r.url = url;
		    r.dataType = "json";
		    r.success = t._onStopVmEnd;
		    $.ajax(r);
		},
		_onStopVmEnd: function (data) {
		    var t = this.manager;
		    var vm = this.virtualMachine;
		    vm.operationId = data;
		    vm.updateOperationStatus(OperationStatus.InProgress);
		    t._virtualMachinesViewModel.showInfo("data-stopvm");
		},
	    downloadRDP: function (vm) {
	        var t = this._t;
	        var url = t._serviceUrl + "/RemoteDesktop/" + vm.name() + "?cloudService=" + vm.cloudServiceName();
	        window.location.href = url;
    	}
	}

	window.Duracellko = {
		AzureManager: azureManager
	}

})(window);