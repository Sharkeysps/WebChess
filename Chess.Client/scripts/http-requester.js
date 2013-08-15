/// <reference path="jquery-2.0.2.js" />
/// <reference path="q.js" />

var httpRequester = (function () {
    var makeHttpRequest = function (url, type, data) {
        var deferred = Q.defer();

        var stringifiedData = "";
        if (data) {
            stringifiedData = JSON.stringify(data);
        }

        $.ajax({
            url: url,
            type: type,
            //dataType: "json",
            contentType: "application/json; charset=utf-8",
            /*headers: {
                Accept: "application/json; charset=utf-8",
                "Content-Type": "application/json; charset=utf-8"
            },*/
            data: stringifiedData,
            success: function (result) {
                deferred.resolve(result);
            },

            error: function (errorData) {
                deferred.reject(errorData);
            }
        })

        return deferred.promise;
    }

    var makeHttpGetRequest = function (url) {
        return makeHttpRequest(url, "GET")
    }

    var makeHttpPostRequest = function (url, data) {
        return makeHttpRequest(url, "POST", data)
    }

    return {
        postJson: makeHttpPostRequest,
        getJson: makeHttpGetRequest
    }
}());