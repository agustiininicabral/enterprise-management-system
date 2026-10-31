import { expect, test } from '@playwright/test';

test('registration, login, role escalation protection, ownership and task CRUD', async ({ page }) => {
  const firstEmail = `owner-${Date.now()}@example.com`;
  const secondEmail = `other-${Date.now()}@example.com`;
  const password = 'E2e-Strong-Password-2026';

  await page.goto('/');
  await page.getByRole('button', { name: 'New here? Create an account' }).click();
  await page.getByLabel('Email address').fill(firstEmail);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Create account' }).click();
  await expect(page.getByRole('heading', { name: 'Overview' })).toBeVisible();

  await page.getByLabel('Task title').fill('Private E2E task');
  await page.getByLabel('Task description').fill('Must remain isolated to its owner');
  const createResponsePromise = page.waitForResponse(response =>
    response.url().endsWith('/api/tasks') && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Add task' }).click();
  const createResponse = await createResponsePromise;
  expect(createResponse.ok()).toBeTruthy();
  const taskId = (await createResponse.json()).id as string;
  await expect(page.getByText('Private E2E task')).toBeVisible();

  const escalationAttempt = await page.request.post('http://127.0.0.1:5080/api/auth/register', {
    data: { email: `escalation-${Date.now()}@example.com`, password, role: 'Admin' }
  });
  expect(escalationAttempt.status()).toBe(201);
  const escalationAuth = await escalationAttempt.json();
  const adminEndpointAsSelfAssignedUser = await page.request.get('http://127.0.0.1:5080/api/admin/users', {
    headers: { Authorization: `Bearer ${escalationAuth.accessToken}` }
  });
  expect(adminEndpointAsSelfAssignedUser.status()).toBe(403);

  await page.locator('button[title="Sign out"]').click();
  await page.getByLabel('Email address').fill(secondEmail);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Create account' }).click();
  await expect(page.getByRole('heading', { name: 'Overview' })).toBeVisible();
  await expect(page.getByText('Private E2E task')).toHaveCount(0);

  const secondToken = await page.evaluate(() => sessionStorage.getItem('enterprise.accessToken'));
  expect(secondToken).toBeTruthy();
  const secondUsersTasks = await page.request.get('http://127.0.0.1:5080/api/tasks', {
    headers: { Authorization: `Bearer ${secondToken}` }
  });
  expect(secondUsersTasks.ok()).toBeTruthy();
  expect(await secondUsersTasks.json()).toEqual([]);
  const crossUserTaskRead = await page.request.get(`http://127.0.0.1:5080/api/tasks/${taskId}`, {
    headers: { Authorization: `Bearer ${secondToken}` }
  });
  expect(crossUserTaskRead.status()).toBe(404);

  await page.locator('button[title="Sign out"]').click();
  await page.getByRole('button', { name: 'Already registered? Sign in' }).click();
  await page.getByLabel('Email address').fill(firstEmail);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Sign in' }).click();

  const ownedTask = page.locator('.task-row').filter({ hasText: 'Private E2E task' });
  await expect(ownedTask).toBeVisible();
  await ownedTask.getByLabel('Status for Private E2E task').selectOption('Done');
  await expect(ownedTask.getByLabel('Status for Private E2E task')).toHaveValue('Done');
  await ownedTask.getByRole('button', { name: 'Delete Private E2E task' }).click();
  await expect(page.getByText('Private E2E task')).toHaveCount(0);
});
