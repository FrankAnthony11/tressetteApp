import { User } from "./user";
import { GameMode } from "./enums";

export interface GameSetup {
  id: string;
  isPasswordProtected: boolean;
  users:User[];
  playUntilPoints:number;
  expectedNumberOfPlayers:number;
  typeOfDeck:number;
  gameMode: GameMode;
}
