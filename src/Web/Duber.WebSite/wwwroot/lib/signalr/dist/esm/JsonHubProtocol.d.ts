import { HubMessage, IHubProtocol } from "./IHubProtocol";
import { ILogger } from "./ILogger";
import { TransferFormat } from "./ITransport";
export declare const JSON_HUB_PROTOCOL_NAME: string;
export declare class JsonHubProtocol implements IHubProtocol {
    readonly name: string;
    readonly version: number;
    readonly transferFormat: TransferFormat;
    parseMessages(input: string, logger: ILogger): HubMessage[];
    writeMessage(message: HubMessage): string;
    private isInvocationMessage(message);
    private isStreamItemMessage(message);
    private isCompletionMessage(message);
    private assertNotEmptyString(value, errorMessage);
}
