
/// <reference path="data-persister.js" />
/// <reference path="ui.js" />
/// <reference path="class.js" />

var Chess = Chess || {};

Chess.controller = (function () {
    var AccessController = Class.create({
        init: function (dataPersister, controlerContainer) {
            this.persister = dataPersister;
            this.controlerContainer = controlerContainer;

            Chess.ui.generateLoginButtons(this.controlerContainer);
        },

        isUserLoggedIn: function () {
            return this.persister.isUserLoggedIn();
        },

        loginUser: function () {
            var loginDeferred = Q.defer();

            this.handleLoginProcedure(loginDeferred);

            return loginDeferred.promise;
        },

        logoutUser: function () {
            this.persister.user.logout().then(function () {
                $("#user-nickname").text('');
                $("#main-content").hide();
            }, (function (error) {
                alert(error.responseJSON.Message);
            })).done();
        },

        showScores: function () {
            var self = this;
            this.persister.user.getAllUserScores().then(function (data) {
                self.scoresGrid = new Chess.ui.GridViewControl();
                self.scoresGrid.build(self.controlerContainer, "Scores", "scores-gridview", ["nickname", "score"], [[]]);
                self.scoresGrid.changeData(data);
                $("<button id='close'>Close</button>").appendTo("#scores-gridview").css({ 'position': 'absolute', 'left': '70%' }).on('click', function () {
                    $("#scores-gridview").remove();
                });
            });
        },

        handleLoginProcedure: function (deferred) {
            var self = this;
            var loginControler = $('#btn-login-display');
            //loginControler.off('click');
            loginControler.on('click', function () {
                $("#login-container").hide();
                $("#register-container").hide();
                Chess.ui.generateLoginForm("#login-container");
                $('#btn-login').on('click', function () {
                    var username = $("#login-username-input").val().escape();
                    var password = $("#login-password-input").val().escape();
                    self.persister.user.login(username, password).then(function (data) {
                        $("#user-nickname").text("Hi, " + data.nickname);
                        self.toggleLoginButtons();
                        deferred.resolve();
                    }, function (error) {
                        deferred.reject(error);
                    });
                });
            });

            var registerControler = $('#btn-register-display');
            //registerControler.off('click');
            registerControler.on('click', function () {
                $("#login-container").hide();
                $("#register-container").hide();
                Chess.ui.generateRegisterForm("#register-container");
                $('#btn-register').on('click', function () {
                    var username = $("#register-username-input").val().escape();
                    var nickname = $("#register-nickname-input").val().escape();
                    var password = $("#register-password-input").val().escape();
                    self.persister.user.register(username, nickname, password).then(function (data) {
                        $("#user-nickname").text("Hi, " + data.nickname);
                        self.toggleLoginButtons();
                        deferred.resolve();
                    }, function (error) {
                        deferred.reject(error);
                    });
                });
            });
        },

        toggleLoginButtons: function () {
            $("#btn-login-display").toggle();
            $("#btn-register-display").toggle();
            $("#btn-logout").toggle();
            $("#btn-scores").toggle();
            $("#login-container").hide();
            $("#register-container").hide();
        }
    });

    var GameController = Class.create({
        init: function (dataPersister, controlerContainer) {
            this.dataPersister = dataPersister;
            this.controlerContainer = controlerContainer;

            this.openGamesList = new Chess.ui.ListControl();
            this.activeGamesList = new Chess.ui.ListControl();
            this.createdGamesList = new Chess.ui.ListControl();
            this.createGameControl = new Chess.ui.CreateGameControl();
            this.messagesList = new Chess.ui.ListControl();
            this.battleControl = new Chess.ui.BattleControl();
            //this.joinGameControl = null;

            //this._joinGameIndex = -1;
            this._scheduledDataUpdates = new Array();
            this._userData = {};
            this._activeGames = {};
            this._openGames = {};
            this._gameUserUnits = {};
            this._gameOpponentUnits = {};
            this._currentGameId = -1;
            this._messages = []
            this._fieldData;
            this.unitId = -1;
            this.targetPosition = {};
        },
        startGame: function () {
            this.stopGame();
            this._userData = this.dataPersister.user.getCurrentUserData();
            var self = this;
            this.initializeControls().then(function () {
                self.attachHandlers();
                self.scheduleDataUpdates();
            });
        },

        initializeControls: function () {

            var self = this;

            this.createGameControl.build(self.controlerContainer, "Create new game.", "create-game-container");

            var promisesArr = new Array();

            //TODO: the following are async, put in array with .all or .then each of them
            promisesArr.push(
            self.openGamesList.build(self.controlerContainer, "Open Games", [], "name", "open-games-list")
            );

            promisesArr.push(
            self.activeGamesList.build(self.controlerContainer, "Active/Full Games", [], "name", "active-games-list")
            );

            self.dataPersister.games.getOpen().then(function (gamesData) {
                self.openGamesList.changeData(gamesData, "name");
            }).done();

            self.dataPersister.games.getCurrentUserActive().then(function (gamesData) {
                self.activeGamesList.changeData(gamesData, "name");
                self.scheduleDataUpdates();
            }).done();

            promisesArr.push(self.battleControl.build(self.controlerContainer, "Game Chess", "game-state-gridview", [[]]).then(function () {
                 if (self._currentGameId != -1) {
                    self.updateCurrentGame();
                }
            }).then(function () {
                // adding message controler after battleControl
                promisesArr.push(
                self.messagesList.build(self.controlerContainer, "Messages", [], "Text", "messages-list")
                );
                $("#messages-list").children("ul").attr("id", "scrolling");
            }));


            /*// adding message controler after battleControl
            promisesArr.push(
            self.messagesList.build(self.controlerContainer, "Messages", [], "text", "messages-list")
            );

            $("#messages-list").children("ul").attr("id", "scrolling");*/

            self.dataPersister.messages.getUserAll().then(function (gamesData) {
                self._messages.push.apply(self._messages, gamesData);
                if (self._messages.length >= 10) {
                    self._messages.splice(10, self._messages.length - 10);
                }
                self.messagesList.changeData(self._messages, "Text");
                document.getElementById("scrolling").scrollTop = document.getElementById("scrolling").scrollHeight;
            }).done();

            return Q.all(promisesArr);
        },

        attachHandlers: function () {
            var self = this;

            this.createGameControl.attachCreateClickHandler(function (gameCreateData) {
                self.dataPersister.games.create(gameCreateData.title, gameCreateData.password)
                    .then(function () {
                        self.createGameControl.clearErrorReport();
                        self.createGameControl.reportSuccess("Game created successfully and will be listed when joined");
                    }, function (error) {
                        self.createGameControl.clearErrorReport();
                        self.createGameControl.reportError(error.responseJSON.Message);
                    }).done();
            });

            this.activeGamesList.attachItemClickHandler(function (itemData) {
                //self.scheduleDataUpdates();
                var clickedActiveGameData = self._activeGames[itemData.itemIndex];

                if (clickedActiveGameData.status == "Active" && clickedActiveGameData.creator == self._userData.nickname) {
                    self.dataPersister.games.start(clickedActiveGameData.id).then(function () {
                        self._currentGameId = clickedActiveGameData.id;
                        self.updateCurrentGame();
                    });
                }

                else {
                    self._currentGameId = clickedActiveGameData.id;
                    self.updateCurrentGame();
                }
            });

            /*this.openGamesList.attachItemClickHandler(function (itemData) {
                if (self._joinGameIndex != itemData.itemIndex) {
                    if (self.joinGameControl) {
                        self.joinGameControl.deleteFromDom();
                    }

                    self._joinGameIndex = itemData.itemIndex;
                    self.joinGameControl = new Chess.ui.JoinGameControl();
                    self.joinGameControl.buildAfterContent("Game Join", "game-join-container", itemData.item).then(function () {
                        self.stopDataUpdates();

                        self.joinGameControl.attachJoinClickHandler(function (joinData) {
                            var gameId = self._openGames[itemData.itemIndex].id;
                            self.dataPersister.games.join(gameId, joinData.password).then(function () {
                                self.scheduleDataUpdates();
                                self.joinGameControl.deleteFromDom();
                                self._joinGameIndex = -1;

                                self._currentGameId = gameId;
                                self.updateCurrentGame();
                            });
                        });
                    }).done();
                }
            });*/

            /*this.battleControl.attachImgClickHandler(function (idData) {
                if (this.unitId == -1) {
                    //if(this._fieldData
                    this.unitId = idData;
                }
                else {
                    this.targetPosition = idData;
                }
                if (this.unitId != -1 && this.targetPosition != -1) {

                    /*self.dataPersister.guesses.make(guessData.number, self._currentGameId).then(function () {
                        self.updateCurrentGame();
                    }, (function (error) {
                        alert(error.responseJSON.Message);
                    })).done();
                }
            });*/

            /*this.makeGuessControl.attachGuessClickHandler(function (guessData) {
                self.dataPersister.guesses.make(guessData.number, self._currentGameId).then(function () {
                    self.updateCurrentGame();
                }, (function (error) {
                    alert(error.responseJSON.Message);
                })).done();
            });*/
        },

        updateCurrentGame: function () {
            var self = this;
            
            this.dataPersister.games.field(this._currentGameId).then(function (gameData) {
                console.log(gameData);
                self._fieldData = gameData;
                self.processCurrentGameData(gameData);
            }, (function (error) {
                alert(error.responseJSON.Message);
            })).then(function () {
                self.battleControl.attachImgClickHandler(function (idData) {
                    var targetUnitId;
                    if (self.unitId == -1) {
                        // TODO I have no time to check is action possible
                        if (idData.unitId != "") {
                            self.unitId = idData.unitId;
                        }
                    }
                    else {
                        if (idData.unitId != "") {
                            targetUnitId = idData.unitId;
                        }
                        self.targetPosition = idData.position;
                    }

                    if (self.unitId != -1 && !jQuery.isEmptyObject(self.targetPosition)) {
                        //if (targetUnitId == undefined) {
                            self.dataPersister.battle.move({ "figureId": self.unitId, "position": self.targetPosition }, self._fieldData.gameId).then(function () {
                                self.updateCurrentGame();
                            }, (function (error) {
                                alert(error.responseJSON.Message);
                            })).done();
                       // }
                        /*else {
                            if (targetUnitId != self.unitId) {
                                self.dataPersister.battle.attack({ "unitId": self.unitId, "position": self.targetPosition }, self._fieldData.gameId).then(function () {
                                    self.updateCurrentGame();
                                }, (function (error) {
                                    alert(error.responseJSON.Message);
                                })).done();
                            }
                            else {
                                self.dataPersister.battle.defend(self.unitId, self._fieldData.gameId).then(function () {
                                    self.updateCurrentGame();
                                }, (function (error) {
                                    alert(error.responseJSON.Message);
                                })).done();
                            }
                        }*/
                        self.unitId = -1;
                        self.targetPosition = {};
                    }
                }, true)
            });
        },

        scheduleDataUpdates: function () {
            var self = this;

            this._scheduledDataUpdates.push(setInterval(function () {
                self.dataPersister.games.getOpen().then(function (games) {
                    self._openGames = games;
                    self.openGamesList.changeData(self._openGames, "name")
                }, (function (error) {
                    alert(error.responseJSON.Message);
                })).done();
            }, 2000));

            this._scheduledDataUpdates.push(setInterval(function () {
                self.dataPersister.games.getCurrentUserActive().then(function (activeGames) {
                    self.processActiveGamesData(activeGames);
                }, (function (error) {
                    alert(error.responseJSON.Message);
                })).done();
            }, 2000));

            this._scheduledDataUpdates.push(setInterval(function () {
                self.dataPersister.messages.getUserUnread().then(function (messages) {
                    self._messages.push.apply(self._messages, messages);
                    self.messagesList.changeData(self._messages, "Text");
                    for (var i in messages) {
                        if (messages[i].gameId == self._currentGameId) {
                            document.getElementById("scrolling").scrollTop = document.getElementById("scrolling").scrollHeight;
                            self.updateCurrentGame();
                        }
                    }
                }, (function (error) {
                    alert(error.responseJSON.Message);
                })).done();
            }, 2000));
        },

        processCurrentGameData: function (gameData) {
            var userColor = "";

            if (this._userData.nickname == gameData.white.nickname) {
                this._gameUserUnits = gameData.white.units;
                this._gameOpponentUnits = gameData.black.units;
                userColor = "white";
            }
            else {
                this._gameUserUnits = gameData.black.units;
                this._gameOpponentUnits = gameData.white.units;
                userColor = "black";
            }
            
            var battleTable = new Array();
            for (var i = 0; i < 8; i++) {
                battleTable.push(new Array({},{},{},{},{},{},{},{}));
            }

            var position;
            for (var i in this._gameUserUnits) {
                var currUserUnits = this._gameUserUnits[i];
                position = currUserUnits.position;
                battleTable[position.row - 1][position.col - 1] = currUserUnits;            
            }

            for (var i in this._gameOpponentUnits) {
                var currOpponentUnits = this._gameOpponentUnits[i];
                position = currOpponentUnits.position;
                battleTable[position.row - 1][position.col - 1] = currOpponentUnits;
            }

            this.battleControl.mainHeader = "Game \"" + gameData.title + "\"";
            this.battleControl.changeData(battleTable, userColor);

            if (gameData.inTurn == userColor) {
                //this.makeGuessControl.show();
            }
            else {
                //this.makeGuessControl.hide();
            }
        },

        processActiveGamesData: function (activeGames) {
            for (i in activeGames) {
                if (activeGames[i].status == "Active" && activeGames[i].creator == this._userData.nickname) {
                    activeGames[i].representation = activeGames[i].name + " - click to start";
                }
                else {
                    if (activeGames[i].id == this._currentGameId) {
                        activeGames[i].representation = activeGames[i].name;
                    }
                    activeGames[i].representation = activeGames[i].name;
                }
            }

            this._activeGames = activeGames;
            this.activeGamesList.changeData(activeGames, "representation");
        },

        stopGame: function () {
            this.stopDataUpdates();

            this._scheduledDataUpdates = new Array();
        },

        stopDataUpdates: function () {
            for (i in this._scheduledDataUpdates) {
                var intervalId = this._scheduledDataUpdates[i];
                clearInterval(intervalId);
            }
        }
    });

    return {
        getAccessController: function (dataPersister, controlerContainer) { return new AccessController(dataPersister, controlerContainer); },
        getGameController: function (dataPersister, controlerContainer) { return new GameController(dataPersister, controlerContainer); }
    }
}());