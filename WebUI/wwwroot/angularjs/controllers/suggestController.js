app
    .directive('fileAutoUpload', ['$parse', function ($parse) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                const fn = $parse(attrs.fileAutoUpload);
                element.bind('change', function () {
                    const file = element[0].files[0];
                    if (file) {
                        scope.$apply(function () {
                            fn(scope, { file: file });
                        });
                    }
                });
            }
        };
    }])
    .controller('suggestController', ['$scope', '$routeParams', '$rootScope', '$http', 'ajaxService', '$location', 'NgTableParams', 'Upload', '$timeout',
        function ($scope, $routeParams, $rootScope, $http, ajaxService, $location, NgTableParams, Upload, $timeout) {
            "use strict";
            var self = this;

            self.recording = false;
            self.status = 'Ready';
            self.blob = null;
            self.blobUrl = null;
            let mediaRecorder = null;
            let chunks = [];

            var panelAnalyze = document.getElementById('panelAnalyze');
            var panelChooseResume = document.getElementById('panelChooseResume');
            self.panelSkills = false;
            self.panelAfterFindJob = false;

            var tablesSkills = document.getElementById('tablesSkills');
            var tablesEducations = document.getElementById('tablesEducations');
            var tablesExperiences = document.getElementById('tablesExperiences');
            var tablesCertificates = document.getElementById('tablesCertificates');
            var tablesBasic = document.getElementById('tablesBasic');

            self.selectText = 'Select All';
            self.readyToSend = [];

            self.TotalApplications = 0;
            self.Rejects = 0;
            self.Passed = 0;
            self.Interviews = 0;

            self.item = {
                ResponseText: '',
                SuggestId: '',
            };
            self.newJob = {
                Title: '',
                Company: '',
                Location: '',
                Link: '',
                JobType: 4,
            };
            self.search = {
                Title: '',
                Company: '',
                Date: '',
                SuggestType: '',
            };

            self.newJobsLinkedIn = [];
            self.newJobsStepStone = [];
            self.newJobsXing = [];

            self.resetItem = function () {
                self.item = {
                    ResponseText: '',
                    SuggestId: '',
                };
            };
            self.initializeController = function () {
                ajaxService.WhoseMenu();

                self.jobTypes = jobType;
                self.suggestTypes = suggestType;
                self.jobTrackerSearchs = jobTrackerSearch;

                if ($location.path().includes("/user/jobTracker")) {
                    self.getAllNewJobsByUserId();
                    self.getAllWaitingJobsByUserId();
                    self.getAllReadyToSendsByUserId();
                    self.getAllSentSuggestByUserId();
                }
            };

            self.updateQuestions = function () {
                const model = {
                    SuggestId: self.item.SuggestId,
                    Questions: self.Questions.map((obj) => {
                        return {
                            Id: obj.Id,
                            Answer: obj.Answer
                        };
                    }),
                }
                return ajaxService.ajaxPost($rootScope.domain + "/api/Suggest/UpdateQuestions", model).then(function (data) {
                    if (data.status != 200)
                        return;
                    location.reload();
                });
            };
            self.addResponse = function () {
                const model = {
                    ResponseText: self.item.ResponseText,
                    SuggestId: self.item.SuggestId,
                }
                return ajaxService.ajaxPost($rootScope.domain + "/api/Suggest/AddResponse", model).then(function (data) {
                    if (data.status != 200)
                        return;
                    location.reload();
                });
            };
            self.getRow = function (row) {
                self.item.SuggestId = row.Id;
                self.Responses = row.Responses;
                self.CoverLetter = row.CoverLetter;
                self.Job = row;

                self.Questions = row.Questions;
            };
            self.getAllSentSuggestByUserId = function () {
                return ajaxService.ajaxGet($rootScope.domain + "/api/Suggest/GetAllSentSuggestByUserId").then(function (data) {
                    if (data.status != 200)
                        return;
                    self.suggests = data.data;

                    self.TotalApplications = self.suggests.length;
                    self.Rejects = self.suggests.filter(m => m.SuggestType == 2).length;
                    self.Passed = self.suggests.filter(m => m.SuggestType == 3).length;
                    self.Interviews = self.suggests.filter(m => m.SuggestType == 4).length;
                });
            };
            self.searchSentSuggestByUserId = function () {
                const model = {
                    Title: self.search.Title,
                    Company: self.search.Company,
                    Date: self.search.Date,
                    SuggestType: self.search.SuggestType,
                };

                return ajaxService.ajaxPost($rootScope.domain + "/api/Suggest/SearchSentSuggestByUserId", model).then(function (data) {
                    if (data.status != 200)
                        return;
                    self.suggests = data.data;

                    var li = self.suggests;
                    if (document.getElementById('chkLinkedIn').checked)
                        li.concat(self.suggests.filter(m => m.JobType == 0));
                    else
                        li = li.filter(m => m.JobType != 0);

                    if (document.getElementById('chkXing').checked)
                        li.concat(self.suggests.filter(m => m.JobType == 1));
                    else
                        li = li.filter(m => m.JobType != 1);

                    if (document.getElementById('chkStepstone').checked)
                        li.concat(self.suggests.filter(m => m.JobType == 2));
                    else
                        li = li.filter(m => m.JobType != 2);

                    self.suggests = li;

                    var li = self.allNewJobs;
                    if (document.getElementById('chkLinkedIn').checked)
                        li.concat(self.newJobs.filter(m => m.JobType == 0));
                    else
                        li = li.filter(m => m.JobType != 0);

                    if (document.getElementById('chkXing').checked)
                        li.concat(self.newJobs.filter(m => m.JobType == 1));
                    else
                        li = li.filter(m => m.JobType != 1);

                    if (document.getElementById('chkStepstone').checked)
                        li.concat(self.newJobs.filter(m => m.JobType == 2));
                    else
                        li = li.filter(m => m.JobType != 2);

                    self.newJobs = li;
                });
            };
            self.getAllWaitingJobsByUserId = function () {
                return ajaxService.ajaxGet($rootScope.domain + "/api/Suggest/GetAllWaitingJobsByUserId").then(function (data) {
                    if (data.status != 200)
                        return;
                    self.waitingJobs = data.data;
                });
            };
            self.getAllReadyToSendsByUserId = function () {
                return ajaxService.ajaxGet($rootScope.domain + "/api/Suggest/GetAllReadyToSendsByUserId").then(function (data) {
                    if (data.status != 200)
                        return;
                    self.readyToSend = data.data;
                });
            };
            self.getAllNewJobsByUserId = function () {
                return ajaxService.ajaxGet($rootScope.domain + "/api/Suggest/GetAllNewJobsByUserId").then(function (data) {
                    if (data.status != 200)
                        return;
                    self.allNewJobs = data.data;
                    self.newJobs = data.data;
                });
            };
            self.selectAllJobs = function () {
                if (self.selectText == 'Select All') {
                    self.newJobs.forEach(function (item) {
                        item.IsSelected = true;
                    });

                    self.selectText = 'Unselect All';
                }
                else {
                    self.newJobs.forEach(function (item) {
                        item.IsSelected = false;
                    });

                    self.selectText = 'Select All';
                }
            };
            self.selectJob = function (row) {
                //row.IsSelected = !row.IsSelected;
                window.open(row.Link, '_blank');
            };
            self.sendApplication = function () {
                const model = {
                    Id: self.Job.Id,
                    Title: self.Job.Title,
                    Company: self.Job.Company,
                    Location: self.Job.Location,
                    Link: self.Job.Link,
                    CoverLetter: self.Job.CoverLetter,
                    JobType: self.Job.JobType,
                }
                return ajaxService.ajaxPost($rootScope.domain + "/api/Suggest/SendApplication", model).then(function (data) {
                    if (data.status != 200)
                        return;

                    if ($location.path().includes("/user/jobTracker"))
                        location.reload();
                    else {
                        SuccessMsg('Application Send Successfully');
                        $('#coverLetterModal').modal('hide');
                    }
                });
            };
            self.addToReady = function () {
                self.readyToSend = self.newJobs.filter(m => m.IsSelected);

                const model = {
                    Jobs: self.readyToSend,
                }
                return ajaxService.ajaxPost($rootScope.domain + "/api/Suggest/GenerateCoverLetter", model).then(function (data) {
                    if (data.status != 200)
                        return;
                    SuccessMsg('CoverLetter Generate Successfully. see in Ready To Send tab');

                    self.readyToSend = data.data;
                });
            };
            self.addNewJobToReady = function () {
                self.readyToSend = [];
                self.readyToSend.push(self.newJob);

                const model = {
                    Jobs: self.readyToSend,
                }
                return ajaxService.ajaxPost($rootScope.domain + "/api/Suggest/GenerateCoverLetter", model).then(function (data) {
                    if (data.status != 200)
                        return;
                    SuccessMsg('CoverLetter Generate Successfully. see in Ready To Send tab');

                    self.readyToSend = data.data;

                    $('#addJobModal').modal('hide');
                });
            };
            self.getNewJobsAndGenerateCoverLetter = function () {
                return ajaxService.ajaxGet($rootScope.domain + "/api/Suggest/GetNewJobsAndGenerateCoverLetter").then(function (data) {
                    if (data.status != 200)
                        return;

                    self.panelSkills = false;
                    self.newJobs = data.data;
                    self.panelAfterFindJob = true;

                    self.newJobsLinkedIn = self.newJobs.filter(m => m.JobType == 0);
                    self.newJobsXing= self.newJobs.filter(m => m.JobType == 1);
                    self.newJobsStepStone = self.newJobs.filter(m => m.JobType == 2);
                });
            };
            //
            self.result = [];
            var selectedFile;
            self.resumeAnalyze1 = async function (file) {
                selectedFile = file;

                panelAnalyze.style.display = 'block';
            };
            self.resumeAnalyze2 = async function () {
                const fd = new FormData();
                fd.append("pdf", selectedFile);
                fd.append("TokenId", ajaxService.getCookie('TokenId'));

                try {
                    self.startChangeTabs(5500);

                    $http.post('/api/Suggest/ResumeAnalyze', fd, { headers: { 'Content-Type': undefined } })
                        .then(function (resp) {

                            panelAnalyze.style.display = 'none';
                            panelChooseResume.style.display = 'none';
                            self.panelSkills = true;

                            self.result = resp.data;
                        });
                } catch (err) {
                    console.error(err);
                }
            };
            //self.send = async function (file) {
            //    if (!file) { return; }

            //    const fd = new FormData();
            //    fd.append("pdf", file);
            //    fd.append("TokenId", ajaxService.getCookie('TokenId'));

            //    try {
            //        self.startChangeTabs(5500);

            //        $http.post('/api/Suggest/GenerateResumeAndGetNewJobs', fd, { headers: { 'Content-Type': undefined } })
            //            .then(function (resp) {
            //                self.result = resp.data;
            //            });
            //    } catch (err) {
            //        console.error(err);
            //    }
            //};
            self.startChangeTabs = async function (seconds) {
                self.changeTabs(2);
                await new Promise(resolve => setTimeout(resolve, seconds));

                self.changeTabs(3);
                await new Promise(resolve => setTimeout(resolve, seconds));

                self.changeTabs(4);
                await new Promise(resolve => setTimeout(resolve, seconds));

                self.changeTabs(5);
            };
            self.changeTabs = function (second) {
                document.getElementById('tab1').classList.remove('active');
                document.getElementById('tab2').classList.remove('active');
                document.getElementById('tab3').classList.remove('active');
                document.getElementById('tab4').classList.remove('active');
                document.getElementById('tab5').classList.remove('active');

                if (second == 2) {
                    document.getElementById('tab2').classList.remove('inactive');
                    document.getElementById('tab2').classList.add('active');
                }
                else if (second == 3) {
                    document.getElementById('tab3').classList.remove('inactive');
                    document.getElementById('tab3').classList.add('active');
                }
                else if (second == 4) {
                    document.getElementById('tab4').classList.remove('inactive');
                    document.getElementById('tab4').classList.add('active');
                }
                else if (second == 5) {
                    document.getElementById('tab5').classList.remove('inactive');
                    document.getElementById('tab5').classList.add('active');
                }
            };
            self.showPanel = function (panelName) {
                if (panelName == 'skill') {
                    tablesSkills.style.display = 'block';
                    tablesEducations.style.display = 'none';
                    tablesExperiences.style.display = 'none';
                    tablesCertificates.style.display = 'none';
                    tablesBasic.style.display = 'none';
                }
                else if (panelName == 'education') {
                    tablesSkills.style.display = 'none';
                    tablesEducations.style.display = 'block';
                    tablesExperiences.style.display = 'none';
                    tablesCertificates.style.display = 'none';
                    tablesBasic.style.display = 'none';
                }
                else if (panelName == 'experience') {
                    tablesSkills.style.display = 'none';
                    tablesEducations.style.display = 'none';
                    tablesExperiences.style.display = 'block';
                    tablesCertificates.style.display = 'none';
                    tablesBasic.style.display = 'none';
                }
                else if (panelName == 'certificate') {
                    tablesSkills.style.display = 'none';
                    tablesEducations.style.display = 'none';
                    tablesExperiences.style.display = 'none';
                    tablesCertificates.style.display = 'block';
                    tablesBasic.style.display = 'none';
                }
                else if (panelName == 'basic') {
                    tablesSkills.style.display = 'none';
                    tablesEducations.style.display = 'none';
                    tablesExperiences.style.display = 'none';
                    tablesCertificates.style.display = 'none';
                    tablesBasic.style.display = 'block';
                }
            };
            self.downloadResume = function () {
                window.open($rootScope.domain + "/DownloadResume?Email=" + self.item.Email, '_blank');
            };
            self.selectTemplate = function (id) {
                if (document.getElementById(id).style.border.includes('dotted')) {
                    document.getElementById(id).style.border = '2px solid darkorange';
                    self.btnDownload = true;
                }
                else {
                    document.getElementById(id).style.border = '2px dotted darkorange';
                    self.btnDownload = false;
                }
            };
            ///

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
                    const resp = await $http.post('/api/User/ProcessUserDetailVoice', fd, {
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
            self.processUserDetailChat = function () {
                const model = {
                    Chat: self.Chat,
                }
                return ajaxService.ajaxPost($rootScope.domain + "/api/User/ProcessUserDetailChat", model).then(function (data) {
                    if (data.status != 200)
                        return;
                    location.reload();
                });
            };
        }]);
