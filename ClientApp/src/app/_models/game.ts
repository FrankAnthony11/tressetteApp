import { User } from './user';
import { Player } from './player';
import { Card } from './card';

export interface Game {
  id: string;
  players: Player[];
  userTurnToPlay: User;
  gameEnded: boolean;
  deck: Card[];
  cardsPlayed: Card[];
  cardsDrew: Card[];
  playUntilPoints: number;
}
