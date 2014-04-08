/// <reference path="jquery-2.1.0.js" />
/// <reference path="knockout-3.1.0.js" />

(function (window, undefined) {

    var virtualMachine = function (manager) {
        this._t = this;
        this._manager = manager;
        this.name = ko.observable("");
        this.cloudServiceName = ko.observable("");
        this.status = ko.observable(null);
        this.statusDisplayName = ko.computed(
            function () {
                var status = this.status();
                var displayName = this._statusDisplayNames[status];
                return (typeof (displayName) == "undefined") ? status : displayName;
            }, this);
        this.canStartVM = ko.computed(function () { return this.status() == "StoppedVM" || this.status() == "StoppedDeallocated" }, this);
        this.canStopVM = ko.computed(function () { return this.status() == "ReadyRole" }, this);
        this.canDownloadRDP = ko.computed(function () { return this.status() == "ReadyRole" }, this);
        this.rowClassName = ko.computed(
            function () {
                if (this.status() == "StoppedVM" || this.status() == "StoppedDeallocated") {
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
		rowClassName: null,
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
	        t._manager.refresh();
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
			
			$(document)
                .ajaxStart(function () { $("#errorPanel").hide(); $.blockUI({ message: "Processing..." }); })
                .ajaxStop($.unblockUI)
                .ajaxError(t._processAjaxError);

			ko.applyBindings(t._virtualMachinesViewModel, $("#azureManagerContainer", t._container).get(0));
		},
		refresh: function () {
			var t = this._t;
			var url = t._serviceUrl + "/VirtualMachines";
			$.ajax({
				context: t,
				url: url,
				dataType: "json",
				success: t._onVirtualMachinesLoaded
			});
		},
		_processAjaxError: function (event, jqXHR, ajaxSettings, thrownError) {
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
		_onVirtualMachinesLoaded: function (data) {
			var t = this._t;
			t._virtualMachines.removeAll();
			for (var i = 0; i < data.length; i++) {
				var vmData = data[i];
				var vm = new virtualMachine(t);
				vm.name(vmData.Name);
				vm.cloudServiceName(vmData.CloudServiceName);
				vm.status(vmData.Status);
				t._virtualMachines.push(vm);
			}
		},
		startVM: function (vm) {
		    var t = this._t;
		    var url = t._serviceUrl + "/StartVM/" + vm.name() + "?cloudService=" + vm.cloudServiceName();
		    $.ajax({
		        context: t,
		        url: url,
		        dataType: "json",
		        success: t._onStartVmEnd
		    });
		},
		_onStartVmEnd: function (data) {
		    var t = this._t;
		    t._virtualMachinesViewModel.showInfo("data-startvm");
		    t.refresh();
		},
		stopVM: function (vm) {
		    var t = this._t;
		    var url = t._serviceUrl + "/ShutdownVM/" + vm.name() + "?cloudService=" + vm.cloudServiceName();
		    $.ajax({
		        context: t,
		        url: url,
		        dataType: "json",
		        success: t._onStopVmEnd
		    });
		},
		_onStopVmEnd: function (data) {
		    var t = this._t;
		    t._virtualMachinesViewModel.showInfo("data-stopvm");
		    t.refresh();
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