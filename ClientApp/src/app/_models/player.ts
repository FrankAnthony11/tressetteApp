import { Card } from './card';

export interface Player {
  cards: Card[];
  connectionId: string;
  calculatedPoints: number;
}
