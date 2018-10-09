import { TypeOfMessage } from './enums';
import { User } from "./user";

export interface ChatMessage{
    username:string;
    text:string;
    typeOfMessage:TypeOfMessage
}