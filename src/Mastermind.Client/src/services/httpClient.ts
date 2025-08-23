import axios, { type AxiosRequestConfig, type AxiosResponse } from 'axios';

/** Remove a trailing slash from a path for consistency. */
function removeTrailingSlash(path: string): string {
	return path.endsWith('/') ? path.slice(0, -1) : path;
}

/** Remove a leading slash from a path for consistency. */
function removeLeadingSlash(path: string): string {
	return path.startsWith('/') ? path.slice(1) : path;
}

/** Resolve the base URL for all requests. */
function resolveBaseUrl(): string {
	const root = '/';
	return removeTrailingSlash(root);
}

/** Build a full URL from the base and input path. */
function buildUrl(path: string): string {
	return `${resolveBaseUrl()}/${removeLeadingSlash(path)}`;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type ApiResponse<T = any, D = any> = AxiosResponse<T, D>;

/** Optional per-request defaults merged into every call. */
const baseConfig: AxiosRequestConfig = {};

/** GET helper. */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export async function httpGet<Response = any, Config = any>(
	path: string,
	config?: AxiosRequestConfig<Config>
): Promise<ApiResponse<Response>> {
	return axios.get<Response, ApiResponse<Response>>(buildUrl(path), {
		...baseConfig,
		...(config ?? {}),
	});
}

/** POST helper. */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export async function httpPost<Response = any, Body = any, Config = any>(
	path: string,
	body: Body,
	config?: AxiosRequestConfig<Config>
): Promise<ApiResponse<Response>> {
	return axios.post<Response, ApiResponse<Response>, Body>(
		buildUrl(path),
		body,
		{
			...baseConfig,
			...(config ?? {}),
		}
	);
}

/** PUT helper. */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export async function httpPut<Response = any, Body = any, Config = any>(
	path: string,
	body: Body,
	config?: AxiosRequestConfig<Config>
): Promise<ApiResponse<Response>> {
	return axios.put<Response, ApiResponse<Response>, Body>(
		buildUrl(path),
		body,
		{
			...baseConfig,
			...(config ?? {}),
		}
	);
}

/** DELETE helper. */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export async function httpDelete<Response = any, Config = any>(
	path: string,
	config?: AxiosRequestConfig<Config>
): Promise<ApiResponse<Response>> {
	return axios.delete<Response, ApiResponse<Response>>(buildUrl(path), {
		...baseConfig,
		...(config ?? {}),
	});
}

export default {
	get: httpGet,
	post: httpPost,
	put: httpPut,
	del: httpDelete,
};
