"use strict";
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Object.defineProperty(exports, "__esModule", { value: true });
var ILogger_1 = require("./ILogger");
var NullLogger = /** @class */ (function () {
    function NullLogger() {
    }
    NullLogger.prototype.log = function (logLevel, message) {
    };
    return NullLogger;
}());
exports.NullLogger = NullLogger;
var ConsoleLogger = /** @class */ (function () {
    function ConsoleLogger(minimumLogLevel) {
        this.minimumLogLevel = minimumLogLevel;
    }
    ConsoleLogger.prototype.log = function (logLevel, message) {
        if (logLevel >= this.minimumLogLevel) {
            switch (logLevel) {
                case ILogger_1.LogLevel.Error:
                    console.error(ILogger_1.LogLevel[logLevel] + ": " + message);
                    break;
                case ILogger_1.LogLevel.Warning:
                    console.warn(ILogger_1.LogLevel[logLevel] + ": " + message);
                    break;
                case ILogger_1.LogLevel.Information:
                    console.info(ILogger_1.LogLevel[logLevel] + ": " + message);
                    break;
                default:
                    console.log(ILogger_1.LogLevel[logLevel] + ": " + message);
                    break;
            }
        }
    };
    return ConsoleLogger;
}());
exports.ConsoleLogger = ConsoleLogger;
var LoggerFactory = /** @class */ (function () {
    function LoggerFactory() {
    }
    LoggerFactory.createLogger = function (logging) {
        if (logging === undefined) {
            return new ConsoleLogger(ILogger_1.LogLevel.Information);
        }
        if (logging === null) {
            return new NullLogger();
        }
        if (logging.log) {
            return logging;
        }
        return new ConsoleLogger(logging);
    };
    return LoggerFactory;
}());
exports.LoggerFactory = LoggerFactory;
//# sourceMappingURL=Loggers.js.map