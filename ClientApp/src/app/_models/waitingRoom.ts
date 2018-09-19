import { User } from "./user";

export interface WaitingRoom {
  id: string;
  users:User[];
  playUntilPoints:number;
  expectedNumberOfPlayers:number;
}
