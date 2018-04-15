import { DataReceived, TransportClosed } from "./Common";
import { HttpClient } from "./HttpClient";
import { ILogger } from "./ILogger";
import { ITransport, TransferFormat } from "./ITransport";
export declare class LongPollingTransport implements ITransport {
    private readonly httpClient;
    private readonly accessTokenFactory;
    private readonly logger;
    private readonly logMessageContent;
    private url;
    private pollXhr;
    private pollAbort;
    private shutdownTimeout;
    private running;
    constructor(httpClient: HttpClient, accessTokenFactory: () => string | Promise<string>, logger: ILogger, logMessageContent: boolean);
    connect(url: string, transferFormat: TransferFormat): Promise<void>;
    private poll(url, transferFormat);
    send(data: any): Promise<void>;
    stop(): Promise<void>;
    onreceive: DataReceived;
    onclose: TransportClosed;
}
