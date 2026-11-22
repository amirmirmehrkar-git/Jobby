namespace WebUI.Helper
{
    public class Prompts
    {
        public static string PromptGetElements = @"
You are given the full HTML source of a job-application page (a fully-rendered HTML fragment or full page).
Your task is to extract every input/control that a user must fill or select to submit the application form.
Return **ONLY** a JSON array. Each array item must be an object with exactly these fields:
  - inputLabel: string (the human label for the field; prefer <label for=...>, aria-label, placeholder or surrounding text)
  - inputSelector: string (a CSS selector that can be used directly in Playwright's page.Locator(selector); prefer id, name, data-* or a unique combination; if selector cannot be perfectly unique, choose a usable selector)
  - inputOptions: array of objects { ""value"": string, ""label"": string } (empty array for non-option fields)
  - inputType: string (one of: text, email, tel, password, file, date, number, textarea, select, radio, checkbox, hidden, other)

Rules:
- Include: <input> (various types), <select>, <textarea>, radio groups and checkbox groups (group them by name and provide options).
- For radio/checkbox groups return one item with inputType 'radio' or 'checkbox', inputSelector should target the group (e.g., \'input[name = 'gender']\') and inputOptions should list each option's value and label.
- Do not include purely decorative inputs or elements with type = 'hidden' unless they are required for the form submission(if hidden and required, include with inputType= 'hidden').
- Do not return any additional text — ONLY the JSON array.
If no form controls found, return an empty JSON array [].
";

        public static string PromptFindNextOrFinalButton = @"
You are given a full HTML page (or fragment). Your job is to detect whether the page contains a button or link related to progressing or finalizing an application process.

You must identify buttons/links that mean either:
- 'Continue', 'Next', 'Proceed', 'Go to Next Step', 'ادامه', 'بعدی', etc. → IsFinal = false
- 'Submit', 'Send Application', 'Finish', 'Finalize', 'ارسال', 'ثبت نهایی', 'پایان', etc. → IsFinal = true

Rules:
- Inspect <button>, <a>, <input type='submit|button|image'>, elements with role='button', or onclick handlers.
- RETURN **ONLY** a JSON object (no extra text) with exactly these two fields:
  { ""IsFinal"": boolean, ""selector"": string }

Selector rules:
- selector must be a **very specific CSS selector** that uniquely identifies the element, suitable for Playwright's page.Locator(selector).
- When constructing the selector, use all useful attributes to make it as specific and unique as possible:
    * Prefer id, if available (e.g., #submitBtn)
    * Otherwise, combine tag name + class names + key attributes (data-*, href, name, aria-label, etc.)
    * You may include :has-text('...') if needed.
    * Avoid overly broad selectors like 'button' or 'a[href*='next']' that may match multiple elements.
- The selector should ideally match only one element.

Classification:
- If the element text or attributes imply 'submit', 'send', 'finish', 'final', 'complete', 'ارسال', 'ثبت نهایی', mark IsFinal = true.
- If the element implies 'next', 'continue', 'ادامه', 'بعدی', mark IsFinal = false.
- If both appear or it’s unclear, choose the one most prominent by semantic meaning.

If no relevant element found:
Return: { ""IsFinal"": false, ""selector"": """" }

Do NOT include any explanations or comments — only the JSON object.

Now analyze the following HTML and return the JSON only.
";

        public static string AnalyzeHtml = @"
You are given a full HTML page (or fragment). Your job is to determine whether the page contains an 'apply' / 'submit application' button or link.

Rules:
- Inspect clickable elements: <button>, <a>, <input type='submit|button|image'>, elements with role='button', or elements with onclick handlers.
- Consider common translations of 'apply' (e.g., apply, apply now, submit application, ارسال درخواست, ارسال رزومه, ثبت‌نام, 지원하기, 申请, postular, bewerben, etc.).
- RETURN **ONLY** a JSON object (no extra text) with exactly three fields:
  { ""found"": boolean, ""selector"": string, ""newTab"": boolean }

Selector rules:
- selector must be a **very specific CSS selector** that uniquely identifies the element, suitable for direct use in Playwright's page.Locator(selector).
- When constructing the selector, use all useful attributes to make it as specific and unique as possible:
    * Prefer id, if it exists (e.g., #applyButton)
    * Otherwise, combine tag name + class names + key attributes (data-*, href, name, aria-label, etc.)
    * You may include :has-text('...') if needed for disambiguation.
    * Avoid returning overly broad selectors (like 'button' or 'a[href*='apply']') that could match multiple elements.
- The selector must correspond to exactly one element if possible.

New tab detection:
- newTab should be true if activating/clicking the returned selector is expected to open a new browser tab/window.
  Evidence includes:
    * target=""_blank"" (or inside a form with that target)
    * rel contains ""noopener"" or ""noreferrer"" with target = ""_blank""
    * onclick or href uses window.open(), open(), or similar JavaScript calls
    * href points to a different domain combined with target=""_blank""
  If there is strong evidence, set newTab = true; otherwise set newTab = false.

If no relevant element is found:
- Return: { ""found"": false, ""selector"": """", ""newTab"": false }

Do NOT include any explanations or comments — only return the JSON object.

Now analyze the following HTML and return the JSON only.
";
    }
}
