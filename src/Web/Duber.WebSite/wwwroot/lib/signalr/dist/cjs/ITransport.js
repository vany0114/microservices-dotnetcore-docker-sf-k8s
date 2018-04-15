"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
var HttpTransportType;
(function (HttpTransportType) {
    HttpTransportType[HttpTransportType["WebSockets"] = 0] = "WebSockets";
    HttpTransportType[HttpTransportType["ServerSentEvents"] = 1] = "ServerSentEvents";
    HttpTransportType[HttpTransportType["LongPolling"] = 2] = "LongPolling";
})(HttpTransportType = exports.HttpTransportType || (exports.HttpTransportType = {}));
var TransferFormat;
(function (TransferFormat) {
    TransferFormat[TransferFormat["Text"] = 1] = "Text";
    TransferFormat[TransferFormat["Binary"] = 2] = "Binary";
})(TransferFormat = exports.TransferFormat || (exports.TransferFormat = {}));
//# sourceMappingURL=ITransport.js.map