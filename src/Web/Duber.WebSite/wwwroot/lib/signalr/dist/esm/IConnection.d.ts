import { ConnectionClosed, DataReceived } from "./Common";
import { TransferFormat } from "./ITransport";
export interface IConnection {
    readonly features: any;
    start(transferFormat: TransferFormat): Promise<void>;
    send(data: any): Promise<void>;
    stop(error?: Error): Promise<void>;
    onreceive: DataReceived;
    onclose: ConnectionClosed;
}
