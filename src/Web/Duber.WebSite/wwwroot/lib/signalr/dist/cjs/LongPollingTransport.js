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
var AbortController_1 = require("./AbortController");
var Errors_1 = require("./Errors");
var ILogger_1 = require("./ILogger");
var ITransport_1 = require("./ITransport");
var Utils_1 = require("./Utils");
var SHUTDOWN_TIMEOUT = 5 * 1000;
var LongPollingTransport = /** @class */ (function () {
    function LongPollingTransport(httpClient, accessTokenFactory, logger, logMessageContent) {
        this.httpClient = httpClient;
        this.accessTokenFactory = accessTokenFactory || (function () { return null; });
        this.logger = logger;
        this.pollAbort = new AbortController_1.AbortController();
        this.logMessageContent = logMessageContent;
    }
    LongPollingTransport.prototype.connect = function (url, transferFormat) {
        Utils_1.Arg.isRequired(url, "url");
        Utils_1.Arg.isRequired(transferFormat, "transferFormat");
        Utils_1.Arg.isIn(transferFormat, ITransport_1.TransferFormat, "transferFormat");
        this.url = url;
        this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Connecting");
        if (transferFormat === ITransport_1.TransferFormat.Binary && (typeof new XMLHttpRequest().responseType !== "string")) {
            // This will work if we fix: https://github.com/aspnet/SignalR/issues/742
            throw new Error("Binary protocols over XmlHttpRequest not implementing advanced features are not supported.");
        }
        this.poll(this.url, transferFormat);
        return Promise.resolve();
    };
    LongPollingTransport.prototype.poll = function (url, transferFormat) {
        return __awaiter(this, void 0, void 0, function () {
            var pollOptions, closeError, token, pollUrl, response, e_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        this.running = true;
                        pollOptions = {
                            abortSignal: this.pollAbort.signal,
                            headers: {},
                            timeout: 90000,
                        };
                        if (transferFormat === ITransport_1.TransferFormat.Binary) {
                            pollOptions.responseType = "arraybuffer";
                        }
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, , 9, 10]);
                        _a.label = 2;
                    case 2:
                        if (!this.running) return [3 /*break*/, 8];
                        return [4 /*yield*/, this.accessTokenFactory()];
                    case 3:
                        token = _a.sent();
                        if (token) {
                            // tslint:disable-next-line:no-string-literal
                            pollOptions.headers["Authorization"] = "Bearer " + token;
                        }
                        _a.label = 4;
                    case 4:
                        _a.trys.push([4, 6, , 7]);
                        pollUrl = url + "&_=" + Date.now();
                        this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) polling: " + pollUrl);
                        return [4 /*yield*/, this.httpClient.get(pollUrl, pollOptions)];
                    case 5:
                        response = _a.sent();
                        if (response.statusCode === 204) {
                            this.logger.log(ILogger_1.LogLevel.Information, "(LongPolling transport) Poll terminated by server");
                            // If we were on a timeout waiting for shutdown, unregister it.
                            clearTimeout(this.shutdownTimeout);
                            this.running = false;
                        }
                        else if (response.statusCode !== 200) {
                            this.logger.log(ILogger_1.LogLevel.Error, "(LongPolling transport) Unexpected response code: " + response.statusCode);
                            // Unexpected status code
                            closeError = new Errors_1.HttpError(response.statusText, response.statusCode);
                            this.running = false;
                        }
                        else {
                            // Process the response
                            if (response.content) {
                                this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) data received. " + Utils_1.getDataDetail(response.content, this.logMessageContent));
                                if (this.onreceive) {
                                    this.onreceive(response.content);
                                }
                            }
                            else {
                                // This is another way timeout manifest.
                                this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Poll timed out, reissuing.");
                            }
                        }
                        return [3 /*break*/, 7];
                    case 6:
                        e_1 = _a.sent();
                        if (!this.running) {
                            // Log but disregard errors that occur after we were stopped by DELETE
                            this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Poll errored after shutdown: " + e_1.message);
                        }
                        else {
                            if (e_1 instanceof Errors_1.TimeoutError) {
                                // Ignore timeouts and reissue the poll.
                                this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Poll timed out, reissuing.");
                            }
                            else {
                                // Close the connection with the error as the result.
                                closeError = e_1;
                                this.running = false;
                            }
                        }
                        return [3 /*break*/, 7];
                    case 7: return [3 /*break*/, 2];
                    case 8: return [3 /*break*/, 10];
                    case 9:
                        // Fire our onclosed event
                        if (this.onclose) {
                            this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Firing onclose event. Error: " + (closeError || "<undefined>"));
                            this.onclose(closeError);
                        }
                        this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Transport finished.");
                        return [7 /*endfinally*/];
                    case 10: return [2 /*return*/];
                }
            });
        });
    };
    LongPollingTransport.prototype.send = function (data) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                if (!this.running) {
                    return [2 /*return*/, Promise.reject(new Error("Cannot send until the transport is connected"))];
                }
                return [2 /*return*/, Utils_1.sendMessage(this.logger, "LongPolling", this.httpClient, this.url, this.accessTokenFactory, data, this.logMessageContent)];
            });
        });
    };
    LongPollingTransport.prototype.stop = function () {
        return __awaiter(this, void 0, void 0, function () {
            var _this = this;
            var deleteOptions, token, response, _a;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        _b.trys.push([0, , 3, 4]);
                        this.running = false;
                        this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) sending DELETE request to " + this.url + ".");
                        deleteOptions = {};
                        return [4 /*yield*/, this.accessTokenFactory()];
                    case 1:
                        token = _b.sent();
                        if (token) {
                            // tslint:disable-next-line:no-string-literal
                            deleteOptions.headers = (_a = {},
                                _a["Authorization"] = "Bearer " + token,
                                _a);
                        }
                        return [4 /*yield*/, this.httpClient.delete(this.url, deleteOptions)];
                    case 2:
                        response = _b.sent();
                        this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) DELETE request accepted.");
                        return [3 /*break*/, 4];
                    case 3:
                        // Abort the poll after 5 seconds if the server doesn't stop it.
                        if (!this.pollAbort.aborted) {
                            this.shutdownTimeout = setTimeout(SHUTDOWN_TIMEOUT, function () {
                                _this.logger.log(ILogger_1.LogLevel.Warning, "(LongPolling transport) server did not terminate within 5 seconds after DELETE request, cancelling poll.");
                                _this.pollAbort.abort();
                            });
                        }
                        return [7 /*endfinally*/];
                    case 4: return [2 /*return*/];
                }
            });
        });
    };
    return LongPollingTransport;
}());
exports.LongPollingTransport = LongPollingTransport;
//# sourceMappingURL=LongPollingTransport.js.map