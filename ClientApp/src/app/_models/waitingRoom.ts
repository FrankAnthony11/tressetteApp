import { User } from "./user";

export interface WaitingRoom {
  id: string;
  password: string;
  users:User[];
  playUntilPoints:number;
  expectedNumberOfPlayers:number;
}
