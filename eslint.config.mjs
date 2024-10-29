import { fileURLToPath } from 'node:url';
import { includeIgnoreFile } from '@eslint/compat';
import dpeslint from '@darkpatternsdigital/eslint-config';
import { tseslint } from '@darkpatternsdigital/eslint-config/types';

export default tseslint.config(
	includeIgnoreFile(fileURLToPath(import.meta.resolve('./.gitignore'))),
	...dpeslint,
);
