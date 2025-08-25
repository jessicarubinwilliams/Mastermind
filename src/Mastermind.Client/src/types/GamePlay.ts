export type GameStatus = 'InProgress' | 'Won' | 'Lost';

export interface GuessDTO {
	attempt: number;
	guess: number[];
	correctNumbers: number;
	correctPositions: number;
	atUtc: string;
}

export interface GameResponse {
	gameId: string;
	status: GameStatus;
	attemptsRemaining: number;
	history: GuessDTO[];
}

export interface GuessRequest {
	guess: number[];
}