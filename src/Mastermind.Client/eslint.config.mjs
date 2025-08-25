import js from '@eslint/js';
import tseslint from 'typescript-eslint';
import vue from 'eslint-plugin-vue';
import prettier from 'eslint-plugin-prettier';

export default [
	js.configs.recommended,
	...tseslint.configs.recommended,
	...vue.configs['flat/recommended'],
	{
		files: ['**/*.{ts,vue}'],
		languageOptions: {
			parserOptions: {
				parser: tseslint.parser,
				ecmaVersion: 'latest',
				sourceType: 'module',
			},
		},
		plugins: {
			prettier,
		},
		rules: {
			indent: ['error', 4],
			semi: ['error', 'always'],
			quotes: ['error', 'double'],
			'prettier/prettier': 'error',
		},
	},
];
