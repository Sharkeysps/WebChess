/// <reference path="class.js" />
/// <reference path="http-requester.js" />

var Chess = Chess || {};

Chess.persisters = (function () {


    var DataPersister = Class.create({
        init: function (serviceRootUrl) {
            this.serviceRootUrl = serviceRootUrl;
            this.user = new UserPersister(serviceRootUrl + "user/");
            this.games = new GamePersister(serviceRootUrl + "game/", this.user);
            this.battle = new BattlePersister(serviceRootUrl + "figure/", this.user);// dali e figure
            this.messages = new MessagePersister(serviceRootUrl + "messages/", this.user);
        },

        isUserLoggedIn: function () {
            var sessionKey = localStorage.getItem("sessionKey");
            //console.log(sessionKey);
            if (sessionKey && sessionKey != "") {
                return true;
            }
            return false;
        }

    });

    var UserPersister = Class.create({
        init: function (serviceRootUrl) {
            this.serviceRootUrl = serviceRootUrl;
        },

        _setUserData: function (data) {
            localStorage.setItem("sessionKey", data.sessionKey);
            localStorage.setItem("nickname", data.nickname);
            localStorage.setItem("username", data.username);
        },

        _getSessionKey: function () {
            return localStorage.getItem("sessionKey");
        },

        _getNickname: function () {
            return localStorage.getItem("nickname");
        },

        _getUsername: function () {
            return localStorage.getItem("username");
        },

        _clearUserData: function () {
            localStorage.setItem("sessionKey", "");
            localStorage.setItem("nickname", "");
            localStorage.setItem("username", "");
        },


        getCurrentUserData: function () {
            return {
                username: this._getUsername(),
                nickname: this._getNickname(),
                sessionKey: this._getSessionKey()
            }
        },

        register: function (username, nickname, password) {
            var hash = CryptoJS.SHA1(password).toString()
            var self = this;
            return httpRequester.postJson(this.serviceRootUrl + "register/", {
                username: username,
                nickname: nickname,
                authCode: hash
            }).then(function (data) {
                self._setUserData(data);
                return data;
            }, function (error) {
                return error;
            });
        },

        login: function (username, password) {
            var hash = CryptoJS.SHA1(password).toString();
            var self = this;

            return httpRequester.postJson(this.serviceRootUrl + "login", {
                username: username,
                authCode: hash
            }).then(function (data) {
                self._setUserData(data);
                return data;
            }, function (error) {
                return error;
            });
        },

        logout: function () {
            var self = this;
            return httpRequester.getJson(this.serviceRootUrl + "logout/" + this._getSessionKey()).
                then(function (data) {
                    self._clearUserData();
                });
        },

        getAllUserScores: function () {
            return httpRequester.getJson(this.serviceRootUrl + "scores/" + this._getSessionKey());
        }
    });

    var GamePersister = Class.create({
        init: function (serviceRootUrl, userPersister) {
            this.serviceRootUrl = serviceRootUrl;
            this.userPersister = userPersister;
        },

        create: function (gameTitle, gamePassword) {
            var gameOptions = {
                Name: gameTitle,
            }

            if (gamePassword) {
                gameOptions.password = CryptoJS.SHA1(gamePassword).toString();
            }

            return httpRequester.postJson(this.serviceRootUrl + "create/" + this.userPersister._getSessionKey(), gameOptions)
        },

        /*join: function (id, gamePassword) {
            var joinOptions = {
                id: id,
            }

            if (gamePassword) {
                joinOptions.password = CryptoJS.SHA1(gamePassword).toString();
            }

            return httpRequester.postJson(this.serviceRootUrl + "join/" + this.userPersister._getSessionKey(), joinOptions)
        },*/

        start: function (gameId) {
            return httpRequester.getJson(this.serviceRootUrl + gameId + "/start/" + this.userPersister._getSessionKey());
        },

        field: function (gameId) {
            return httpRequester.getJson(this.serviceRootUrl + gameId + "/field/" + this.userPersister._getSessionKey());
        },

        getOpen: function () {
            return httpRequester.getJson(this.serviceRootUrl + "open/" + this.userPersister._getSessionKey());
        },

        getCurrentUserActive: function () {
            return httpRequester.getJson(this.serviceRootUrl + "my-active/" + this.userPersister._getSessionKey());
        }
    });

    

    var BattlePersister = Class.create({
        init: function (serviceRootUrl, userPersister) {
            this.serviceRootUrl = serviceRootUrl;
            this.userPersister = userPersister;
        },

        move: function (moveOptions, gameId) {
            return httpRequester.postJson(this.serviceRootUrl + gameId + "/move/" + this.userPersister._getSessionKey(), {
                figureId: moveOptions.figureId,
                position: moveOptions.position
            });
        },

        /*attack: function (attackOptions, gameId) {
            return httpRequester.postJson(this.serviceRootUrl + gameId + "/attack/" + this.userPersister._getSessionKey(), {
                unitId: attackOptions.unitId,
                position: attackOptions.position
            });
        },

        defend: function (unitId, gameId) {
            return httpRequester.postJson(this.serviceRootUrl + gameId + "/defend/" + this.userPersister._getSessionKey(), unitId);
        }*/
    });

    var MessagePersister = Class.create({
        init: function (serviceRootUrl, userPersister) {
            this.serviceRootUrl = serviceRootUrl;
            this.userPersister = userPersister;
        },

        getUserUnread: function () {
            return httpRequester.getJson(this.serviceRootUrl + "unread/" + this.userPersister._getSessionKey())
        },

        getUserAll: function () {
            return httpRequester.getJson(this.serviceRootUrl + "all/" + this.userPersister._getSessionKey())
        }
    });

    return {
        getPersister: function (url) { return new DataPersister(url); },
    }
}());
