import { expect, test } from '@playwright/test';

test('register, create, update and delete a task against the live API', async ({ page }) => {
  const email = `e2e-${Date.now()}@example.com`;

  await page.goto('/');
  await page.getByRole('button', { name: 'New here? Create an account' }).click();
  await page.getByLabel('Email address').fill(email);
  await page.getByLabel('Password').fill('E2e-Strong-Password-2026');
  await page.getByRole('button', { name: 'Create account' }).click();

  await expect(page.getByRole('heading', { name: 'Overview' })).toBeVisible();
  await page.getByLabel('Task title').fill('E2E regression task');
  await page.getByLabel('Task description').fill('Created through the browser against SQL Server');
  await page.getByRole('button', { name: 'Add task' }).click();

  const task = page.locator('.task-row').filter({ hasText: 'E2E regression task' });
  await expect(task).toBeVisible();
  await task.getByLabel('Status for E2E regression task').selectOption('Done');
  await expect(task.getByLabel('Status for E2E regression task')).toHaveValue('Done');

  await task.getByRole('button', { name: 'Delete E2E regression task' }).click();
  await expect(page.getByText('E2E regression task')).toHaveCount(0);
});
