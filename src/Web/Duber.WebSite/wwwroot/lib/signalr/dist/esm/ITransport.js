// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
export var HttpTransportType;
(function (HttpTransportType) {
    HttpTransportType[HttpTransportType["WebSockets"] = 0] = "WebSockets";
    HttpTransportType[HttpTransportType["ServerSentEvents"] = 1] = "ServerSentEvents";
    HttpTransportType[HttpTransportType["LongPolling"] = 2] = "LongPolling";
})(HttpTransportType || (HttpTransportType = {}));
export var TransferFormat;
(function (TransferFormat) {
    TransferFormat[TransferFormat["Text"] = 1] = "Text";
    TransferFormat[TransferFormat["Binary"] = 2] = "Binary";
})(TransferFormat || (TransferFormat = {}));
//# sourceMappingURL=ITransport.js.map