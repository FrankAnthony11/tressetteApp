import { User } from './user';
import { Player } from './player';
import { Card } from './card';
import { CardAndUser } from './cardAndUser';

export interface Game {
  id: string;
  players: Player[];
  userTurnToPlay: User;
  gameEnded: boolean;
  deck: Card[];
  cardsPlayed: CardAndUser[];
  cardsDrew: CardAndUser[];
  playUntilPoints: number;
}
