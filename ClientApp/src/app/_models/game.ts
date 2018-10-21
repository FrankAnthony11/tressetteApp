import { GameSetup } from './gameSetup';
import { User } from './user';
import { Player } from './player';
import { Card } from './card';
import { CardAndUser } from './cardAndUser';
import { Team } from './team';

export interface Game {
  gameSetup:GameSetup;
  players: Player[];
  spectators: User[];
  userTurnToPlay: User;
  gameEnded: boolean;
  gameStarted: boolean;
  roundEnded: boolean;
  isFirstRound: boolean;
  deckSize: number;
  myCards: Card[];
  teams: Team[];
  cardsPlayed: CardAndUser[];
  cardsPlayedPreviousRound: CardAndUser[];
  cardsDrew: CardAndUser[];
}
