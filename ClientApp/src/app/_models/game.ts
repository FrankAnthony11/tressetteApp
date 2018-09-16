import { Player } from './player';
import { Card } from './card';

export interface Game {
  id: string;
  player1: Player;
  player2: Player;
  gameEnded: boolean;
  deck: Card[];
  cardsPlayed: Card[];
}
