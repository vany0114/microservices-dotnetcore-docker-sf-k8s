// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
import { LogLevel } from "./ILogger";
var NullLogger = /** @class */ (function () {
    function NullLogger() {
    }
    NullLogger.prototype.log = function (logLevel, message) {
    };
    return NullLogger;
}());
export { NullLogger };
var ConsoleLogger = /** @class */ (function () {
    function ConsoleLogger(minimumLogLevel) {
        this.minimumLogLevel = minimumLogLevel;
    }
    ConsoleLogger.prototype.log = function (logLevel, message) {
        if (logLevel >= this.minimumLogLevel) {
            switch (logLevel) {
                case LogLevel.Error:
                    console.error(LogLevel[logLevel] + ": " + message);
                    break;
                case LogLevel.Warning:
                    console.warn(LogLevel[logLevel] + ": " + message);
                    break;
                case LogLevel.Information:
                    console.info(LogLevel[logLevel] + ": " + message);
                    break;
                default:
                    console.log(LogLevel[logLevel] + ": " + message);
                    break;
            }
        }
    };
    return ConsoleLogger;
}());
export { ConsoleLogger };
var LoggerFactory = /** @class */ (function () {
    function LoggerFactory() {
    }
    LoggerFactory.createLogger = function (logging) {
        if (logging === undefined) {
            return new ConsoleLogger(LogLevel.Information);
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
export { LoggerFactory };
//# sourceMappingURL=Loggers.js.map