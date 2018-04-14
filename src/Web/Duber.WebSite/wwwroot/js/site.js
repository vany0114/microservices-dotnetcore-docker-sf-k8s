var DuberWebSite = (function () {

    var _directionsDisplay, _directionsService;
    var _map, _currentPointIndex = 0;
    var _currentPositionMarker;
    var _route, _directions = new Array();
    var _places = [];
    var _from, _to;

    var _updateCurrentPosition = (position) => {
        var currentPosition = new google.maps.LatLng(position.Latitude, position.Longitude);

        _currentPositionMarker.setMap(null);
        _currentPositionMarker = new google.maps.Marker({
            position: currentPosition,
            map: _map,
            title: "Current posistion",
            animation: google.maps.Animation.BOUNCE,
            icon: 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'
        });
    };

    var _test = () => {
        _currentPointIndex = _currentPointIndex + 5;
        var lat = _directions[_currentPointIndex].lat();
        var long = _directions[_currentPointIndex].lng();

        _updateCurrentPosition({ Latitude: lat, Longitude: long });
    };

    var _getDirections = () => {
        _route = _route.routes[0];
        var legs = _route.legs;
        for (i = 0; i < legs.length; i++) {
            var steps = legs[i].steps;
            for (j = 0; j < steps.length; j++) {
                var nextSegment = steps[j].path;
                for (k = 0; k < nextSegment.length; k++) {
                    _directions.push(nextSegment[k]);
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

    var _init = () => {
        $(".test-button").click(() => { _test(); return; });
        $(".trip-from").change(_fromChanged);
        $(".trip-to").change(_toChanged);

        _from = $(".trip-from").val();
        _to = $(".trip-to").val();
    }

    var initMap = (places) => {
        _init();
        _places = places;

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

    return {
        initMap: initMap
    };
})();