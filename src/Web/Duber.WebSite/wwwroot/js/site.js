// sorry for this ugly JS.
var DuberWebSite = (function () {

    var _directionsDisplay, _directionsService;
    var _map, _currentPointIndex = 0;
    var _currentPositionMarker;
    var _route, _directions = new Array();
    var _places = [];
    var _from, _to;
    var _simulateTripUrl, _iconUrl;

    var _getDirections = () => {
        _route = _route.routes[0];
        var legs = _route.legs;
        for (i = 0; i < legs.length; i++) {
            var steps = legs[i].steps;
            for (j = 0; j < steps.length; j++) {
                var nextSegment = steps[j].path;
                for (k = 0; k < nextSegment.length; k++) {
                    _directions.push({ Latitude: nextSegment[k].lat(), Longitude: nextSegment[k].lng(), Description: "" });
                }
            }
        }
    };

    var _calcRoute = () => {
        if (_from === _to) { return; }
        var fromPoint = _.find(_places, ['Description', _from]);
        var toPoint = _.find(_places, ['Description', _to]);

        var start = new google.maps.LatLng(fromPoint.Latitude, fromPoint.Longitude);
        var end = new google.maps.LatLng(toPoint.Latitude, toPoint.Longitude);

        var request = {
            origin: start,
            destination: end,
            travelMode: 'DRIVING'
        };

        _directions = [];
        _currentPointIndex = 0;
        _directionsService.route(request,
            function (response, status) {
                if (status === 'OK') {
                    _directionsDisplay.setDirections(response);
                    _route = _directionsDisplay.getDirections();
                    _getDirections();
                } else {
                    alert("directions request failed, status=" + status);
                }
            });
    };

    var _fromChanged = (event) => {
        _from = event.currentTarget.value;
        _calcRoute();
    };

    var _toChanged = (event) => {
        _to = event.currentTarget.value;
        _calcRoute();
    }

    var _simulate = () => {

        $("#errors").addClass("hidden");
        $("#info").addClass("hidden");
        $('#errors-body').empty();
        $("#simulate").prop("disabled", true);

        $.post(_simulateTripUrl,
            {
                User: $("#User").val(),
                Driver: $("#Driver").val(),
                From: $("#From").val(),
                To: $("#To").val(),
                Directions: _directions
            },
            () => {
                console.log("Simulation request sent succesfully.");
            })
            .fail((response) => {
                console.log("Simulation request returned a bad request");
                console.log(response);

                $("#simulate").prop("disabled", false);
                $("#errors").removeClass("hidden");
                if (response.responseJSON) {
                    response.responseJSON.forEach((name) => {
                        var li = document.createElement('li');
                        li.innerHTML += name;
                        $('#errors-body').append(li);
                    });
                } else {
                    var li = document.createElement('li');
                    li.innerHTML += response.statusText;
                    $('#errors-body').append(li);
                }
            });
    };

    var _initMap = () => {
        _directionsDisplay = new google.maps.DirectionsRenderer();
        _directionsService = new google.maps.DirectionsService();
        _currentPositionMarker = new google.maps.Marker({});

        var mapOptions = {
            zoom: 7
        }

        _map = new google.maps.Map(document.getElementById('map'), mapOptions);
        _directionsDisplay.setMap(_map);
        _calcRoute();
    };

    var updateCurrentPosition = (position) => {
        var currentPosition = new google.maps.LatLng(position.latitude, position.longitude);

        _currentPositionMarker.setMap(null);
        _currentPositionMarker = new google.maps.Marker({
            position: currentPosition,
            map: _map,
            title: "Current posistion",
            animation: google.maps.Animation.BOUNCE,
            //icon: 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'
            icon: _iconUrl
        });
    };

    var notifyTripStatus = (message) => {
        let className = "alert-info";
        if (message === "Finished") {
            className = "alert-success";
            $("#simulate").prop("disabled", false);
        }

        $("#info").removeClass("alert-success");
        $("#info").removeClass("alert-info");
        $("#info").addClass(className);
        $("#info").removeClass("hidden");
        $("#info").html(`The trip has been ${message}!`);
    };

    var init = (places, simulateTripUrl, iconUrl) => {
        $(".trip-from").change(_fromChanged);
        $(".trip-to").change(_toChanged);
        $("#simulate").click(() => {
            _simulate();
            return;
        });

        _from = $(".trip-from").val();
        _to = $(".trip-to").val();

        _places = places;
        _simulateTripUrl = simulateTripUrl;
        _iconUrl = iconUrl;
        _initMap();
    };

    return {
        init: init,
        updateTripPosition: updateCurrentPosition,
        notifyTripStatus: notifyTripStatus
    };
})();