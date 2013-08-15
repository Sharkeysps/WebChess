/// <reference path="jquery-2.0.2.js" />

var Chess = Chess || {};

Chess.ui = (function () {
    
    // generate UI for login and register forms
    var generateLoginButtons = function (selector) {
        $("<span id='user-nickname'></span>").appendTo(selector);
        $("<button id='btn-login-display'>LogIn</button>").appendTo(selector).addClass('button').css({ 'position': 'absolute', 'left': '80%', 'top': '0' });
        $("<button id='btn-register-display'>Register</button>").appendTo(selector).addClass('button').css({ 'position': 'absolute', 'left': '90%', 'top': '0' });
        $("<button id='btn-logout'>LogOut</button>").appendTo(selector).addClass('button').css({ 'position': 'absolute', 'left': '90%', 'top': '0' });
        $("<button id='btn-scores'>Scores</button>").appendTo(selector).addClass('button').css({ 'position': 'absolute', 'left': '80%', 'top': '0' });
    }
    var generateLoginForm = function (selector) {
        var element = $(selector);
        element.css({ 'position': 'fixed', 'left': '80%' });
        element.html('<label for="login-username-input">Username: </label>' +
        '    <input type="text" id="login-username-input" />' +
        '    <label for="login-password-input">Password: </label>' +
        '    <input type="password" id="login-password-input" />' +
        '    <button id="btn-login">Login</button>').slideDown(1000);
    }
    var generateRegisterForm = function (selector) {
        var element = $(selector);
        element.css({ 'position': 'fixed', 'left': '86%' });
        element.html('<div id="register-container">'+
        '    <label for="register-username-input">Username: </label>'+
        '    <input type="text" id="register-username-input" />'+
        '    <label for="register-nickname-input">Nickname: </label>'+
        '    <input type="text" id="register-nickname-input" />'+
        '    <label for="register-password-input">Password: </label>'+
        '    <input type="password" id="register-password-input" />'+
        '    <button id="btn-register">Register</button>'+
        '</div>').slideDown(1000);        
    }

    var ListControl = Class.create({
        build: function (selector, header, data, dataDisplayPropertyName, id) {

            //var self = this;
            this.rootElement = $(selector);
            this.listElements = new Array();
            var deferred = Q.defer();
            this.container = $("<div id='" + id + "' class='control-container'></div>");
            this.rootElement.append(this.container);
            $("<h3 class='control-header'></h3>").appendTo(this.container).text(header);
            this.listContainer = $("<ul class='control-data-container'></ul>").appendTo(this.container)
            deferred.resolve();
            this.changeData(data, dataDisplayPropertyName);
            return deferred.promise;
        },

        changeData: function (newData, dataDisplayPropertyName) {
            this.listContainer.html("");
            this.listElements = [];

            if (newData) {
                for (var i in newData) {
                    var listElement = $("<li>" + newData[i][dataDisplayPropertyName] + "</li>").appendTo(this.listContainer);
                    
                    this.listElements.push(listElement);
                }
            }
        },

        attachItemClickHandler: function (handler, removePreviousHandlers) {
            var self = this;

            if (removePreviousHandlers) {
                $(this.container).off("click");
            }

            self.container.on("click", "li", function (event) {
                var itemData = {
                    itemIndex: $(this).index(),
                    item: $(this)
                }
                handler(itemData);
            })
        }
    });

    var CreateGameControl = Class.create({
        build: function (selector, header, id) {
            this.rootElement = $(selector);
            var deferred = Q.defer();
            this.container = $("<div id='" + id + "' class='control-container'></div>");
            this.rootElement.append(this.container);
            this.container.html("<h3 class='control-header'>" + header + "</h3>" +
                '<label for="create-game-title-input">Title: </label>' +
                '<input type="text" id="create-game-title-input" />' +
                '<label for="create-game-password-input">Password: </label>' +
                '<input type="password" id="create-game-password-input" />' +
                '<button id="create-game-button">Create Game</button>');
            deferred.resolve();
            return deferred.promise;                       
        },

        getTitleText: function () {
            return $("#create-game-title-input").val().escape();
        },
        
        getPasswordText: function () {
            return $("#create-game-password-input").val().escape();
        },

        attachCreateClickHandler: function (handler, removePreviousHandlers) {

            var self = this;

            if (removePreviousHandlers) {
                $(this.container).off("click");
            }

            $(this.container).on("click", "#create-game-button", function () {
                var gameCreateData = {
                    title: self.getTitleText(),
                    password: self.getPasswordText(),
                };

                handler(gameCreateData);
                $("#create-game-title-input").val;
                $("#create-game-password-input").val;
            });
        },

        reportSuccess: function (message) {
            var successMessage = $("<p class='success-message' style='color:green'>" + message + "</p>");
            this.container.append(successMessage);
            successMessage.fadeOut(2000, function () {
                successMessage.remove();
            });
        },

        reportError: function (errorMessage) {
            this.container.append("<p class='error-message' style='color:red'>" + errorMessage + "</p>");
        },

        clearErrorReport: function () {
            var errorMessages = this.container.children(".error-message");
            errorMessages.fadeOut(200, function () {
                errorMessages.remove();
            });
        }
    });

    
    /*var JoinGameControl = Class.create({
        buildAfterContent: function (header, id, listItem) {
            this.rootElement = $("<div class='control-container'></div>");
            var deferred = Q.defer();
            $(listItem).append(this.rootElement);
            this.container = $("<div id='" + id + "'></div>");
            this.rootElement.append(this.container);
            this.container.html("<h3 class='control-header'>" + header + "</h3>" +
                '<label for="game-join-password-input">Password: </label>' +
                '<input type="password" id="game-join-password-input" />' +
                '<button id="game-join-button">Join</button>');
            deferred.resolve();

            return deferred.promise;
        },

        getRoot: function () {
            return this.container;
        },

        deleteFromDom: function () {
            $("#game-join-container").parent().remove();
        },

        getPasswordText: function () {
            return $("#game-join-password-input").val().escape();
        },

        attachJoinClickHandler: function (handler, removePreviousHandlers) {

            var self = this;

            if (removePreviousHandlers) {
                $(this.container).off("click");
            }

            $(this.container).on("click", "#game-join-button", function () {
                var gameLoginData = {
                    password: self.getPasswordText(),
                };

                handler(gameLoginData);
                return false;
            });
        },

        reportError: function (errorMessage) {
            this.container.append("<p class='error-message' style='color:red'>" + errorMessage + "</p>");
        },

        clearErrorReport: function () {
            this.container.children(".error-message").remove();
        }
    });*/

    var GridViewControl = Class.create({
        build: function (selector, mainHeader, id, headers, dataMatrix) {
            var container = $("<div id='" + id + "' class='control-container'></div>");
            $(selector).append(container);
            this.rootElement = container;

            this.mainHeader = mainHeader;
            this.headers = headers;

            this.changeData(dataMatrix);
        },

        changeData: function (data) {
            var resultHtml = "<h2>" + this.mainHeader + "</h2><table>";

            resultHtml += "<thead>";
            resultHtml += "<tr>";
            for (var i in this.headers) {
                resultHtml += "<th>" + this.headers[i] + "</th>";
            }
            resultHtml += "</tr>";
            resultHtml += "</thead>";

            resultHtml += "<tbody>";
            for (var row in data) {
                var currRow = data[row];
                resultHtml += "<tr>";
                for (var col in currRow) {
                    resultHtml += "<td>" + currRow[col] + "</td>";
                }
                resultHtml += "</tr>";
            }
            resultHtml += "</tbody>";
            resultHtml += "</table>";

            this.rootElement.html(resultHtml);
        }
    });

    var BattleGridViewControl = Class.create({
        build: function (selector, mainHeader, id, dataMatrix) {
            this.container = $("<div id='" + id + "' class='control-container'></div>");
            $(selector).append(this.container);
            this.rootElement = this.container;

            this.mainHeader = mainHeader;
            this.changeData(dataMatrix);
            var deferred = Q.defer();
            deferred.resolve();

            return deferred.promise;
        },

        changeData: function (data, owner) {
            var resultHtml = "<h2>" + this.mainHeader + "</h2><table>";
                        
            resultHtml += "<tbody>";
            //for (var row = 0; row < data.length; row++) {
            for(var row in data){
                var currRow = data[row];
                resultHtml += "<tr>";
                //for (var col = 0; currRow.length; col++) {
                //for(var col in currRow){
                //    if (!jQuery.isEmptyObject(currRow[col])) {
                //        var defendMode = "";
                //        if (currRow[col].mode == "defend" && currRow[col].owner == owner) {
                //            defendMode = "D";
                //        }
                //        if (currRow[col].type == "warrior" && currRow[col].owner == "red") {
                //            resultHtml += '<td id="' + row + '-' + col + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/warriorRed.png)">' + defendMode + '</span> </td>';
                //        }
                //        else if (currRow[col].type == "ranger" && currRow[col].owner == "red") {
                //            resultHtml += '<td id="' + row + '-' + col + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/archerRed.png)">' + defendMode + '</span> </td>';
                //        }
                //        else if (currRow[col].type == "warrior" && currRow[col].owner == "blue") {
                //            resultHtml += '<td id="' + row + '-' + col + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/warriorBlue.png)">' + defendMode + '</span> </td>';
                //        }
                //        else if (currRow[col].type == "ranger" && currRow[col].owner == "blue") {
                //            resultHtml += '<td id="' + row + '-' + col + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/archerBlue.png)">' + defendMode + '</span> </td>';
                //        }
                //    }
                //    else{
                //        resultHtml += '<td id="' + row + '-' + col + '"></td>';
                //    }
                //}

                for (var col in currRow) {
                    var row1 = parseInt(row) + 1;
                    var col1 = parseInt(col) + 1;
                    if (!jQuery.isEmptyObject(currRow[col])) {
                        
                        if (currRow[col].type == "Pawn" && currRow[col].isWhite == true) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MRedPawn.png)">'+ '</span> </td>';
                        }
                        else if (currRow[col].type == "King" && currRow[col].isWhite == true) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MRedKing.png)">' + '</span> </td>';
                        }
                        else if (currRow[col].type == "Queen" && currRow[col].isWhite == true) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MRedQueen.png)">' + '</span> </td>';
                        }
                        else if (currRow[col].type == "Rook" && currRow[col].isWhite == true) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MRedRook.png)">' + '</span> </td>';
                        }
                        else if (currRow[col].type == "Bishop" && currRow[col].isWhite == true) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MRedBishop.png)">' + '</span> </td>';
                        }
                        else if (currRow[col].type == "Knight" && currRow[col].isWhite == true) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MRedKnight.png)">' + '</span> </td>';
                        }
                        else if (currRow[col].type == "Pawn" && currRow[col].isWhite == false) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MBluePawn.png)">' + '</span> </td>';
                        }
                        else if (currRow[col].type == "King" && currRow[col].isWhite == false) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MBlueKing.png)">' + '</span> </td>';
                        }
                        else if (currRow[col].type == "Queen" && currRow[col].isWhite == false) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MBlueQueen.png)">' + '</span> </td>';
                        }
                        else if (currRow[col].type == "Rook" && currRow[col].isWhite == false) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MBlueRook.png)">' + '</span> </td>';
                        }
                        else if (currRow[col].type == "Bishop" && currRow[col].isWhite == false) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MBlueBishop.png)">' + '</span> </td>';
                        }
                        else if (currRow[col].type == "Knight" && currRow[col].isWhite == false) {
                            resultHtml += '<td id="' + row1 + '-' + col1 + '"><span id="' + currRow[col].id + '" style="background-image: url(../images/MBlueKnight.png)">' + '</span> </td>';
                        }
                    }
                    else {                        
                        resultHtml += '<td id="' + row1 + '-' + col1 + '"></td>';
                    }
                }


                resultHtml += "</tr>";
            }
            resultHtml += "</tbody>";
            resultHtml += "</table>";

            this.rootElement.html(resultHtml);

            $("<div id='selected-div'></div>").appendTo(this.rootElement).css({ 'position': 'absolute', 'left': 0, 'top': 0 }).hide();
        },

        attachImgClickHandler: function (handler, removePreviousHandlers) {
            
            if (removePreviousHandlers) {
                $(this.container).off("click");
            }

            $("#selected-div").click(function (e) {
                $("td span").each(function () {
                    // check if clicked point (taken from event) is inside element
                    var mouseX = e.pageX;
                    var mouseY = e.pageY;
                    var offset = $(this).offset();
                    var width = $(this).width();
                    var height = $(this).height();

                    if (mouseX > offset.left && mouseX < offset.left + width
                        && mouseY > offset.top && mouseY < offset.top + height)
                        $(this).click(); // force click event
                });
            });

            $(this.container).on("click", "td", function (ev) {
                var gameLoginData;
                if (ev.target.parentNode.id == "") {
                    var n = ev.target.id.split("-");
                    gameLoginData = {
                        position: { row: n[0], col: n[1] },
                        unitId: ev.target.parentNode.id
                    };
                }
                else {
                    var n = ev.target.parentNode.id.split("-");
                    gameLoginData = {
                        position: { row: n[0], col: n[1] },
                        unitId: ev.target.id
                    };
                }

                $("#selected-div").css({ 'position': 'absolute', 'left': $("#" + ev.target.id).offset().left, 'top': $("#" + ev.target.id).offset().top }).toggle();
                    
                handler(gameLoginData);
                return false;
            });
        },
    });

    return {
        generateLoginButtons: generateLoginButtons,
        generateLoginForm: generateLoginForm,
        generateRegisterForm: generateRegisterForm,
        ListControl: ListControl,
        //JoinGameControl: JoinGameControl,
        CreateGameControl: CreateGameControl,
        GridViewControl: GridViewControl,
        BattleControl: BattleGridViewControl
    }
}());