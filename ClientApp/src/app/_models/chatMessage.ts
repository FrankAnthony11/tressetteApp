import { TypeOfMessage } from './enums';
import { User } from "./user";

export interface ChatMessage{
    user:User;
    text:string;
    typeOfMessage:TypeOfMessage
}