app.controller('experienceController', ['$scope', '$routeParams', '$rootScope', '$http', 'ajaxService', '$location', 'NgTableParams', 'Upload', '$timeout',
    function ($scope, $routeParams, $rootScope, $http, ajaxService, $location, NgTableParams, Upload, $timeout) {
        "use strict";
        var self = this;

        self.recording = false;
        self.status = 'Ready';
        self.blob = null;
        self.blobUrl = null;
        let mediaRecorder = null;
        let chunks = [];

        $rootScope.Experience = {
            Project: '',
            Company: '',
            Role: '',
            Activity: '',
            Benefit: '',
        };
        self.resetItem = function () {
            self.Id = undefined;
            $rootScope.Experience = {
                Project: '',
                Company: '',
                Role: '',
                Activity: '',
                Benefit: '',
            };
        };
        self.initializeController = function () {
            ajaxService.WhoseMenu();
        };

        self.tableParams = new NgTableParams($rootScope.tableParams["experienceController_tableParams"] ||
        {
            page: 1,
            count: 15,
        },
            {
                filterDelay: 300,
                getData: function (params) {
                    $rootScope.tableParams["experienceController_tableParams"] = params._params;

                    let url = $rootScope.domain + "/api/Experience/GetAllByUserId";

                    return ajaxService.ajaxPaged(url, params._params).then(function (data) {
                        if (data.status != 200)
                            return;
                        params.total(data.data.count);
                        return data.data.items;
                    });
                }
            });

        self.addOrUpdate = function () {
            const model = {
                Id: $rootScope.Experience.Id ? $rootScope.Experience.Id : emptyGuid,
                Project: $rootScope.Experience.Project,
                Company: $rootScope.Experience.Company,
                Role: $rootScope.Experience.Role,
                Activity: $rootScope.Experience.Activity,
                Benefit: $rootScope.Experience.Benefit,
            }
            return ajaxService.ajaxPost($rootScope.domain + "/api/Experience/AddOrUpdate", model).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        }
        self.getRow = function (row) {
            $rootScope.Experience.Id = row.Id;
            $rootScope.Experience.Project = row.Project;
            $rootScope.Experience.Company = row.Company;
            $rootScope.Experience.Role = row.Role;
            $rootScope.Experience.Activity = row.Activity;
            $rootScope.Experience.Benefit = row.Benefit;
        };
        self.delete = function () {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Experience/Delete/" + $rootScope.Experience.Id).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };

        //
        self.start = async function () {
            self.status = 'Requesting microphone access ...';
            try {
                const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
                mediaRecorder = new MediaRecorder(stream);
                chunks = [];
                mediaRecorder.ondataavailable = e => { if (e.data && e.data.size) chunks.push(e.data); };
                mediaRecorder.onstop = () => {
                    self.blob = new Blob(chunks, { type: mediaRecorder.mimeType || 'audio/webm' });
                    self.blobUrl = URL.createObjectURL(self.blob);
                    self.status = `Recording ready — Size: ${formatBytes(self.blob.size)}`;
                    $scope.$apply();
                };
                mediaRecorder.start();
                self.recording = true;
                self.status = 'Recording ...';
                $scope.$apply();
            } catch (err) {
                console.error(err);
                self.status = 'Microphone access error';
                $scope.$apply();
            }
        };
        self.stop = function () {
            if (mediaRecorder && self.recording) { mediaRecorder.stop(); self.recording = false; }
        };
        self.send = async function () {
            if (!self.blob) { alert('No Recording File'); return; }
            self.status = 'Sending To AI ...';
            const fd = new FormData();
            fd.append('audio', self.blob, `recording_${Date.now()}.webm`);
            try {
                const resp = await $http.post('/api/Experience/process', fd, {
                    headers: { 'Content-Type': undefined },
                    transformRequest: angular.identity
                });

                location.reload();

            } catch (err) {
                console.error(err);
            }
        };
        function formatBytes(bytes) {
            if (bytes === 0) return '0 B';
            const k = 1024; const sizes = ['B', 'KB', 'MB', 'GB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return (bytes / Math.pow(k, i)).toFixed(2) + ' ' + sizes[i];
        }
        ///
    }]);
