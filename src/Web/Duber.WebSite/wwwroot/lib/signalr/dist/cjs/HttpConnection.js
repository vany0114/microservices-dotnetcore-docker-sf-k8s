"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var HttpClient_1 = require("./HttpClient");
var ILogger_1 = require("./ILogger");
var ITransport_1 = require("./ITransport");
var Loggers_1 = require("./Loggers");
var LongPollingTransport_1 = require("./LongPollingTransport");
var ServerSentEventsTransport_1 = require("./ServerSentEventsTransport");
var Utils_1 = require("./Utils");
var WebSocketTransport_1 = require("./WebSocketTransport");
var HttpConnection = /** @class */ (function () {
    function HttpConnection(url, options) {
        if (options === void 0) { options = {}; }
        this.features = {};
        Utils_1.Arg.isRequired(url, "url");
        this.logger = Loggers_1.LoggerFactory.createLogger(options.logger);
        this.baseUrl = this.resolveUrl(url);
        options = options || {};
        options.accessTokenFactory = options.accessTokenFactory || (function () { return null; });
        options.logMessageContent = options.logMessageContent || false;
        this.httpClient = options.httpClient || new HttpClient_1.DefaultHttpClient(this.logger);
        this.connectionState = 2 /* Disconnected */;
        this.options = options;
    }
    HttpConnection.prototype.start = function (transferFormat) {
        Utils_1.Arg.isRequired(transferFormat, "transferFormat");
        Utils_1.Arg.isIn(transferFormat, ITransport_1.TransferFormat, "transferFormat");
        this.logger.log(ILogger_1.LogLevel.Trace, "Starting connection with transfer format '" + ITransport_1.TransferFormat[transferFormat] + "'.");
        if (this.connectionState !== 2 /* Disconnected */) {
            return Promise.reject(new Error("Cannot start a connection that is not in the 'Disconnected' state."));
        }
        this.connectionState = 0 /* Connecting */;
        this.startPromise = this.startInternal(transferFormat);
        return this.startPromise;
    };
    HttpConnection.prototype.startInternal = function (transferFormat) {
        return __awaiter(this, void 0, void 0, function () {
            var _this = this;
            var token, headers, negotiateResponse, e_1, _a;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        _b.trys.push([0, 7, , 8]);
                        if (!(this.options.transport === ITransport_1.HttpTransportType.WebSockets)) return [3 /*break*/, 2];
                        // No need to add a connection ID in this case
                        this.url = this.baseUrl;
                        this.transport = this.constructTransport(ITransport_1.HttpTransportType.WebSockets);
                        // We should just call connect directly in this case.
                        // No fallback or negotiate in this case.
                        return [4 /*yield*/, this.transport.connect(this.url, transferFormat)];
                    case 1:
                        // We should just call connect directly in this case.
                        // No fallback or negotiate in this case.
                        _b.sent();
                        return [3 /*break*/, 6];
                    case 2: return [4 /*yield*/, this.options.accessTokenFactory()];
                    case 3:
                        token = _b.sent();
                        headers = void 0;
                        if (token) {
                            headers = (_a = {},
                                _a["Authorization"] = "Bearer " + token,
                                _a);
                        }
                        return [4 /*yield*/, this.getNegotiationResponse(headers)];
                    case 4:
                        negotiateResponse = _b.sent();
                        // the user tries to stop the the connection when it is being started
                        if (this.connectionState === 2 /* Disconnected */) {
                            return [2 /*return*/];
                        }
                        return [4 /*yield*/, this.createTransport(this.options.transport, negotiateResponse, transferFormat, headers)];
                    case 5:
                        _b.sent();
                        _b.label = 6;
                    case 6:
                        if (this.transport instanceof LongPollingTransport_1.LongPollingTransport) {
                            this.features.inherentKeepAlive = true;
                        }
                        this.transport.onreceive = this.onreceive;
                        this.transport.onclose = function (e) { return _this.stopConnection(e); };
                        // only change the state if we were connecting to not overwrite
                        // the state if the connection is already marked as Disconnected
                        this.changeState(0 /* Connecting */, 1 /* Connected */);
                        return [3 /*break*/, 8];
                    case 7:
                        e_1 = _b.sent();
                        this.logger.log(ILogger_1.LogLevel.Error, "Failed to start the connection: " + e_1);
                        this.connectionState = 2 /* Disconnected */;
                        this.transport = null;
                        throw e_1;
                    case 8: return [2 /*return*/];
                }
            });
        });
    };
    HttpConnection.prototype.getNegotiationResponse = function (headers) {
        return __awaiter(this, void 0, void 0, function () {
            var negotiateUrl, response, e_2;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        negotiateUrl = this.resolveNegotiateUrl(this.baseUrl);
                        this.logger.log(ILogger_1.LogLevel.Trace, "Sending negotiation request: " + negotiateUrl);
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4 /*yield*/, this.httpClient.post(negotiateUrl, {
                                content: "",
                                headers: headers,
                            })];
                    case 2:
                        response = _a.sent();
                        return [2 /*return*/, JSON.parse(response.content)];
                    case 3:
                        e_2 = _a.sent();
                        this.logger.log(ILogger_1.LogLevel.Error, "Failed to complete negotiation with the server: " + e_2);
                        throw e_2;
                    case 4: return [2 /*return*/];
                }
            });
        });
    };
    HttpConnection.prototype.updateConnectionId = function (negotiateResponse) {
        this.connectionId = negotiateResponse.connectionId;
        this.url = this.baseUrl + (this.baseUrl.indexOf("?") === -1 ? "?" : "&") + ("id=" + this.connectionId);
    };
    HttpConnection.prototype.createTransport = function (requestedTransport, negotiateResponse, requestedTransferFormat, headers) {
        return __awaiter(this, void 0, void 0, function () {
            var transports, _i, transports_1, endpoint, transport, ex_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        this.updateConnectionId(negotiateResponse);
                        if (!this.isITransport(requestedTransport)) return [3 /*break*/, 2];
                        this.logger.log(ILogger_1.LogLevel.Trace, "Connection was provided an instance of ITransport, using that directly.");
                        this.transport = requestedTransport;
                        return [4 /*yield*/, this.transport.connect(this.url, requestedTransferFormat)];
                    case 1:
                        _a.sent();
                        // only change the state if we were connecting to not overwrite
                        // the state if the connection is already marked as Disconnected
                        this.changeState(0 /* Connecting */, 1 /* Connected */);
                        return [2 /*return*/];
                    case 2:
                        transports = negotiateResponse.availableTransports;
                        _i = 0, transports_1 = transports;
                        _a.label = 3;
                    case 3:
                        if (!(_i < transports_1.length)) return [3 /*break*/, 9];
                        endpoint = transports_1[_i];
                        this.connectionState = 0 /* Connecting */;
                        transport = this.resolveTransport(endpoint, requestedTransport, requestedTransferFormat);
                        if (!(typeof transport === "number")) return [3 /*break*/, 8];
                        this.transport = this.constructTransport(transport);
                        if (!(negotiateResponse.connectionId === null)) return [3 /*break*/, 5];
                        return [4 /*yield*/, this.getNegotiationResponse(headers)];
                    case 4:
                        negotiateResponse = _a.sent();
                        this.updateConnectionId(negotiateResponse);
                        _a.label = 5;
                    case 5:
                        _a.trys.push([5, 7, , 8]);
                        return [4 /*yield*/, this.transport.connect(this.url, requestedTransferFormat)];
                    case 6:
                        _a.sent();
                        this.changeState(0 /* Connecting */, 1 /* Connected */);
                        return [2 /*return*/];
                    case 7:
                        ex_1 = _a.sent();
                        this.logger.log(ILogger_1.LogLevel.Error, "Failed to start the transport '" + ITransport_1.HttpTransportType[transport] + "': " + ex_1);
                        this.connectionState = 2 /* Disconnected */;
                        negotiateResponse.connectionId = null;
                        return [3 /*break*/, 8];
                    case 8:
                        _i++;
                        return [3 /*break*/, 3];
                    case 9: throw new Error("Unable to initialize any of the available transports.");
                }
            });
        });
    };
    HttpConnection.prototype.constructTransport = function (transport) {
        switch (transport) {
            case ITransport_1.HttpTransportType.WebSockets:
                return new WebSocketTransport_1.WebSocketTransport(this.options.accessTokenFactory, this.logger, this.options.logMessageContent);
            case ITransport_1.HttpTransportType.ServerSentEvents:
                return new ServerSentEventsTransport_1.ServerSentEventsTransport(this.httpClient, this.options.accessTokenFactory, this.logger, this.options.logMessageContent);
            case ITransport_1.HttpTransportType.LongPolling:
                return new LongPollingTransport_1.LongPollingTransport(this.httpClient, this.options.accessTokenFactory, this.logger, this.options.logMessageContent);
            default:
                throw new Error("Unknown transport: " + transport + ".");
        }
    };
    HttpConnection.prototype.resolveTransport = function (endpoint, requestedTransport, requestedTransferFormat) {
        var transport = ITransport_1.HttpTransportType[endpoint.transport];
        if (transport === null || transport === undefined) {
            this.logger.log(ILogger_1.LogLevel.Trace, "Skipping transport '" + endpoint.transport + "' because it is not supported by this client.");
        }
        else {
            var transferFormats = endpoint.transferFormats.map(function (s) { return ITransport_1.TransferFormat[s]; });
            if (!requestedTransport || transport === requestedTransport) {
                if (transferFormats.indexOf(requestedTransferFormat) >= 0) {
                    if ((transport === ITransport_1.HttpTransportType.WebSockets && typeof WebSocket === "undefined") ||
                        (transport === ITransport_1.HttpTransportType.ServerSentEvents && typeof EventSource === "undefined")) {
                        this.logger.log(ILogger_1.LogLevel.Trace, "Skipping transport '" + ITransport_1.HttpTransportType[transport] + "' because it is not supported in your environment.'");
                    }
                    else {
                        this.logger.log(ILogger_1.LogLevel.Trace, "Selecting transport '" + ITransport_1.HttpTransportType[transport] + "'");
                        return transport;
                    }
                }
                else {
                    this.logger.log(ILogger_1.LogLevel.Trace, "Skipping transport '" + ITransport_1.HttpTransportType[transport] + "' because it does not support the requested transfer format '" + ITransport_1.TransferFormat[requestedTransferFormat] + "'.");
                }
            }
            else {
                this.logger.log(ILogger_1.LogLevel.Trace, "Skipping transport '" + ITransport_1.HttpTransportType[transport] + "' because it was disabled by the client.");
            }
        }
        return null;
    };
    HttpConnection.prototype.isITransport = function (transport) {
        return typeof (transport) === "object" && "connect" in transport;
    };
    HttpConnection.prototype.changeState = function (from, to) {
        if (this.connectionState === from) {
            this.connectionState = to;
            return true;
        }
        return false;
    };
    HttpConnection.prototype.send = function (data) {
        if (this.connectionState !== 1 /* Connected */) {
            throw new Error("Cannot send data if the connection is not in the 'Connected' State.");
        }
        return this.transport.send(data);
    };
    HttpConnection.prototype.stop = function (error) {
        return __awaiter(this, void 0, void 0, function () {
            var e_3;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        this.connectionState = 2 /* Disconnected */;
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4 /*yield*/, this.startPromise];
                    case 2:
                        _a.sent();
                        return [3 /*break*/, 4];
                    case 3:
                        e_3 = _a.sent();
                        return [3 /*break*/, 4];
                    case 4:
                        if (!this.transport) return [3 /*break*/, 6];
                        this.stopError = error;
                        return [4 /*yield*/, this.transport.stop()];
                    case 5:
                        _a.sent();
                        this.transport = null;
                        _a.label = 6;
                    case 6: return [2 /*return*/];
                }
            });
        });
    };
    HttpConnection.prototype.stopConnection = function (error) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                this.transport = null;
                // If we have a stopError, it takes precedence over the error from the transport
                error = this.stopError || error;
                if (error) {
                    this.logger.log(ILogger_1.LogLevel.Error, "Connection disconnected with error '" + error + "'.");
                }
                else {
                    this.logger.log(ILogger_1.LogLevel.Information, "Connection disconnected.");
                }
                this.connectionState = 2 /* Disconnected */;
                if (this.onclose) {
                    this.onclose(error);
                }
                return [2 /*return*/];
            });
        });
    };
    HttpConnection.prototype.resolveUrl = function (url) {
        // startsWith is not supported in IE
        if (url.lastIndexOf("https://", 0) === 0 || url.lastIndexOf("http://", 0) === 0) {
            return url;
        }
        if (typeof window === "undefined" || !window || !window.document) {
            throw new Error("Cannot resolve '" + url + "'.");
        }
        // Setting the url to the href propery of an anchor tag handles normalization
        // for us. There are 3 main cases.
        // 1. Relative  path normalization e.g "b" -> "http://localhost:5000/a/b"
        // 2. Absolute path normalization e.g "/a/b" -> "http://localhost:5000/a/b"
        // 3. Networkpath reference normalization e.g "//localhost:5000/a/b" -> "http://localhost:5000/a/b"
        var aTag = window.document.createElement("a");
        aTag.href = url;
        this.logger.log(ILogger_1.LogLevel.Information, "Normalizing '" + url + "' to '" + aTag.href + "'.");
        return aTag.href;
    };
    HttpConnection.prototype.resolveNegotiateUrl = function (url) {
        var index = url.indexOf("?");
        var negotiateUrl = url.substring(0, index === -1 ? url.length : index);
        if (negotiateUrl[negotiateUrl.length - 1] !== "/") {
            negotiateUrl += "/";
        }
        negotiateUrl += "negotiate";
        negotiateUrl += index === -1 ? "" : url.substring(index);
        return negotiateUrl;
    };
    return HttpConnection;
}());
exports.HttpConnection = HttpConnection;
//# sourceMappingURL=HttpConnection.js.map