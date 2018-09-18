import { User } from "./user";

export interface WaitingRoom {
  id: string;
  user1:User;
  user2:User;
  playUntilPoints:number;
}
