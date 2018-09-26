import { User } from './user';
import { Player } from './player';
import { Card } from './card';
import { CardAndUser } from './cardAndUser';
import { Team } from './team';

export interface Game {
  id: string;
  players: Player[];
  userTurnToPlay: User;
  gameEnded: boolean;
  roundEnded: boolean;
  deckSize: number;
  myCards: Card[];
  teams: Team[];
  cardsPlayed: CardAndUser[];
  cardsDrew: CardAndUser[];
  playUntilPoints: number;
}
