<template>
    <section class="game-board">
        <div v-if="!hasGame || isTerminal" class="buttonRow">
            <button type="button" @click="startGame" :disabled="pending">
                Start Game
            </button>
        </div>

        <div v-if="hasGame">
            <div v-if="game">
                <p><strong>Status:</strong> {{ game.status }}</p>
                <p><strong>Attempts remaining:</strong> {{ game.attemptsRemaining }}</p>
            </div>

            <fieldset v-if="showInputs" :disabled="!showInputs || pending">
                <legend>Enter your guess</legend>
                <div class="guess-row">
                    <input
                        v-for="i in 4"
                        :key="i"
                        type="text"
                        inputmode="numeric"
                        v-model.trim="guess[i - 1]"
                    />
                </div>
            </fieldset>

            <div v-if="showInputs" class="actions-row">
                <button type="button" @click="submitGuess" :disabled="pending || !gameId">
                    Guess
                </button>
                <button type="button" @click="refreshStatus" :disabled="pending || !gameId">
                    Refresh Status
                </button>
            </div>

            <p v-if="isTerminal"><strong>{{ terminalMessage }}</strong></p>

            <p v-if="errorText" role="alert">{{ errorText }}</p>

            <section v-if="game && game.history && game.history.length > 0" class="history-section">
                <h2>Guess history</h2>
                <ol class="history-list">
                    <li v-for="h in game.history" :key="h.attempt" class="history-item">
                        <div>
                            <span><strong>Attempt:</strong> {{ h.attempt }}</span>
                            <span class="spacer"><strong>Guess:</strong> {{ h.guess.join(' ') }}</span>
                            <span class="spacer"><strong>Correct numbers:</strong> {{ h.correctNumbers }}</span>
                            <span class="spacer"><strong>Correct positions:</strong> {{ h.correctPositions }}</span>
                            <span class="spacer"><strong>At:</strong> {{ h.atUtc }}</span>
                        </div>
                    </li>
                </ol>
            </section>
        </div>
    </section>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import httpClient from '@/services/httpClient';
import type { GameResponse, GuessRequest, GameStatus } from '@/types/GamePlay';

const game = ref<GameResponse | null>(null);
const gameId = ref<string | null>(null);
const guess = ref<string[]>(['0', '0', '0', '0']);
const errorText = ref<string | null>(null);
const pending = ref(false);

const hasGame = computed<boolean>(() => game.value !== null);

const status = computed<GameStatus | null>(() => (game.value ? game.value.status : null));
const isTerminal = computed<boolean>(() => status.value === 'Won' || status.value === 'Lost');
const terminalMessage = computed<string>(() => {
    if (status.value === 'Won') return 'You won';
    if (status.value === 'Lost') return 'Game over';
    return '';
});
const showInputs = computed<boolean>(() => !isTerminal.value && hasGame.value);

const startGame = async (): Promise<void> => {
    resetUiState();
    pending.value = true;
    errorText.value = null;
    try {
        const { data } = await httpClient.post<GameResponse>(`/api/Games`);
        game.value = data;
        gameId.value = data.gameId;
    } catch (err: any) {
        errorText.value = readProblemText(err);
    } finally {
        pending.value = false;
    }
};

const submitGuess = async (): Promise<void> => {
    if (!gameId.value) {
        errorText.value = 'Start a game first';
        return;
    }
    pending.value = true;
    errorText.value = null;
    try {
        // Convert string inputs to numbers without any form validation
        const numbers = guess.value.map(g => Number(g));
        const payload: GuessRequest = { guess: numbers };
        const { data } = await httpClient.post<GameResponse>(`/api/Games/${gameId.value}/guesses`, payload);
		console.log('data', data);
        game.value = data;
    } catch (err: any) {
        errorText.value = readProblemText(err);
    } finally {
        pending.value = false;
    }
};

const refreshStatus = async (): Promise<void> => {
    if (!gameId.value) {
        errorText.value = 'Start a game first';
        return;
    }
    pending.value = true;
    errorText.value = null;
    try {
        const { data } = await httpClient.get<GameResponse>(`/api/Games/${gameId.value}`);
        game.value = data;
    } catch (err: any) {
        errorText.value = readProblemText(err);
    } finally {
        pending.value = false;
    }
};

const readProblemText = (err: any): string => {
    const title: string | undefined = err?.response?.data?.title;
    const detail: string | undefined = err?.response?.data?.detail;
    if (title && detail) return `${title}: ${detail}`;
    if (title) return title;
    if (detail) return detail as string;
    return 'Request failed';
};

const resetUiState = (): void => {
    game.value = null;
    gameId.value = null;
    guess.value = ['0', '0', '0', '0'];
    errorText.value = null;
};
</script>
