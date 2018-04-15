import { HttpClient } from "./HttpClient";
import { ILogger } from "./ILogger";
export declare class Arg {
    static isRequired(val: any, name: string): void;
    static isIn(val: any, values: any, name: string): void;
}
export declare function getDataDetail(data: any, includeContent: boolean): string;
export declare function formatArrayBuffer(data: ArrayBuffer): string;
export declare function sendMessage(logger: ILogger, transportName: string, httpClient: HttpClient, url: string, accessTokenFactory: () => string | Promise<string>, content: string | ArrayBuffer, logMessageContent: boolean): Promise<void>;
