import { User } from "./user";

export interface Team{
    name:string;
    users:User[];
    calculatedPoints:number;
}