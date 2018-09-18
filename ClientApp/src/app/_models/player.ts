import { Card } from './card';
import { User } from './user';

export interface Player {
  cards: Card[];
  user:User;
  calculatedPoints: number;
}
