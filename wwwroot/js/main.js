/** @format */

//If we use these functions from .Net, we need to name them with a capital letter,otherwise, we must call them with a lowercase letter.
//We must use static classes for different logic areas. If the logic is general, we can use the InternalFunctions

window.InternalFunctions = class InternalFunctions {
  static async DrawImage(canvas, image, x, y, width, height) {
    const bitmap = await createImageBitmap(new Blob([image]));

    canvas.getContext("2d").drawImage(bitmap, x, y, width, height);

    bitmap.close();
  }

  static SubscribeEvents(dotNetObject) {
    window.removeEventListener("keydown", (e) =>
      PrivateFunctions.onKeyDown(e, dotNetObject)
    );
    window.addEventListener("keydown", (e) =>
      PrivateFunctions.onKeyDown(e, dotNetObject)
    );

    window.removeEventListener("keyup", (e) =>
      PrivateFunctions.onKeyUp(e, dotNetObject)
    );
    window.addEventListener("keyup", (e) =>
      PrivateFunctions.onKeyUp(e, dotNetObject)
    );

    window.removeEventListener("blur", () =>
      PrivateFunctions.onBlur(dotNetObject)
    );
    window.addEventListener("blur", () =>
      PrivateFunctions.onBlur(dotNetObject)
    );
  }

  static WatchClipboard(dotNetObject) {
    PrivateFunctions.watchClipboard(dotNetObject);
  }

  static SetClipboardText(text) {
    if (text == PrivateFunctions.LastClipboardText) {
      return;
    }

    PrivateFunctions.NewClipboardText = text;
  }

  static ToggleFullScreen(canvas) {
    if (document.fullscreenElement) {
      document.exitFullscreen();
    } else {
      canvas.requestFullscreen();
    }
  }
};

class PrivateFunctions {
  static onKeyDown(e, dotNetObject) {
    if (!e.ctrlKey || !e.shiftKey || e.key.toLowerCase() != "i") {
      e.preventDefault();
    }
    dotNetObject.invokeMethodAsync("OnKeyDown", e.key);
  }
  static onKeyUp(e, dotNetObject) {
    e.preventDefault();
    dotNetObject.invokeMethodAsync("OnKeyUp", e.key);
  }
  static onBlur(dotNetObject) {
    dotNetObject.invokeMethodAsync("OnBlur");
  }
  static ClipboardTimer;
  static LastClipboardText;
  static NewClipboardText;
  static watchClipboard(dotNetObject) {
    if (
      !location.protocol.includes("https") &&
      !location.origin.includes("localhost")
    ) {
      console.warn(
        "Clipboard API only works in a secure context (i.e. HTTPS or localhost)."
      );
      return;
    }

    if (!navigator.clipboard?.readText) {
      console.warn("Clipboard API not supported.");
      return;
    }

    if (this.ClipboardTimer) {
      console.log("ClipboardWatcher is already running.");
      return;
    }

    this.ClipboardTimer = setInterval(() => {
      if (!document.hasFocus()) {
        return;
      }

      if (this.NewClipboardText && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(this.NewClipboardText);
        return;
      }

      navigator.clipboard.readText().then((newText) => {
        dotNetObject.invokeMethodAsync("SendClipboardText", newText, false);
      });
    }, 500);
  }
}
