
document.addEventListener("DOMContentLoaded", () => {
    const buttons = document.querySelectorAll(
        "[data-password-toggle]"
    );

    buttons.forEach(button => {
        button.addEventListener("click", () => {
            const inputId = button.dataset.passwordToggle;
            const input = document.getElementById(inputId);

            if (!input) return;

            const isHidden = input.type === "password";

            input.type = isHidden ? "text" : "password";
            button.textContent = isHidden ? "Skjul" : "Vis";
            button.setAttribute(
                "aria-pressed",
                String(isHidden)
            );
            button.setAttribute(
                "aria-label",
                isHidden ? "Skjul passord" : "Vis passord"
            );
        });
    });
});
