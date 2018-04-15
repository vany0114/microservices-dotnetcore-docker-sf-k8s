import { DataReceived, TransportClosed } from "./Common";
export declare enum HttpTransportType {
    WebSockets = 0,
    ServerSentEvents = 1,
    LongPolling = 2,
}
export declare enum TransferFormat {
    Text = 1,
    Binary = 2,
}
export interface ITransport {
    connect(url: string, transferFormat: TransferFormat): Promise<void>;
    send(data: any): Promise<void>;
    stop(): Promise<void>;
    onreceive: DataReceived;
    onclose: TransportClosed;
}
