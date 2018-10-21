import { User } from "./user";

export interface GameSetup {
  id: string;
  isPasswordProtected: boolean;
  users:User[];
  playUntilPoints:number;
  expectedNumberOfPlayers:number;
}
