// Scripted walkthrough for the README demo. Run: node demo/record.mjs .
// Story: accent-insensitive search, a play counted once for its label at 30 listened seconds, and the queue.
const liftCaption = page => page.addStyleTag({ content: "#__demo_caption { bottom: 100px !important; }" }); // keep the now-playing bar visible

export default async ({ page, step, shot, pause }) => {
  await liftCaption(page);
  const search = page.getByRole("searchbox", { name: "Search tracks, artists and albums" });

  await step("Wavelength: music from 300 independent South African labels", async () => {
    await search.waitFor();
  });
  await step("Accents are optional: typing \"cafe\" finds \"Café Kwasa\"", async () => {
    await search.pressSequentially("cafe", { delay: 180 });
    await page.getByRole("button", { name: "Play Café Kwasa", exact: true }).waitFor();
  });
  await shot("search");

  await step("Play streams the audio from the CDN path; the label is paid only after 30 s", async () => {
    await page.getByRole("button", { name: "Play Café Kwasa", exact: true }).click();
    await page.getByRole("button", { name: "Pause" }).waitFor();
    await page.evaluate(() => { document.querySelector("audio").playbackRate = 4; });
  });
  await step("Proof: at 0:30 listened (4× speed here) the play is reported once by its play ID", async () => {
    await page.getByText(/^✓ Counted for /).waitFor({ timeout: 30000 });
    await page.evaluate(() => { document.querySelector("audio").playbackRate = 1; });
    await pause(1600);
  });
  await shot("counted");

  await step("+ Next lines up more tracks without interrupting the song", async () => {
    await search.fill("");
    await search.pressSequentially("salt", { delay: 120 });
    await page.getByRole("button", { name: "Play Salt Road Lullaby next" }).click();
    await pause(500);
    await page.getByRole("button", { name: "Play Salt & Ember next" }).click();
  });
  await step("The queue shows what plays next, and any track can be removed", async () => {
    await page.getByRole("heading", { name: "Up next (2)" }).waitFor();
    await pause(1200);
    await page.getByRole("button", { name: "Remove Salt & Ember from queue" }).click();
    await page.getByRole("heading", { name: "Up next (1)" }).waitFor();
  });
  await shot("queue");
};

export async function mobile({ page, shot, pause }) {
  const search = page.getByRole("searchbox", { name: "Search tracks, artists and albums" });
  await search.fill("cafe");
  await page.getByRole("button", { name: "Play Café Kwasa", exact: true }).click();
  await pause(1500);
  await shot("mobile");
}
