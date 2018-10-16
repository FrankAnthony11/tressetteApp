import { User } from './user';
import { Player } from './player';
import { Card } from './card';
import { CardAndUser } from './cardAndUser';
import { Team } from './team';

export interface Game {
  id: string;
  players: Player[];
  spectators: User[];
  userTurnToPlay: User;
  gameEnded: boolean;
  roundEnded: boolean;
  isFirstRound: boolean;
  deckSize: number;
  myCards: Card[];
  teams: Team[];
  cardsPlayed: CardAndUser[];
  cardsPlayedPreviousRound: CardAndUser[];
  cardsDrew: CardAndUser[];
  playUntilPoints: number;
}
