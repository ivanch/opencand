/**
 * requestAnimationFrame loop for decorative canvas animations.
 *
 * - capped at `maxFps` (default 30)
 * - paused while the tab is hidden (document visibilitychange)
 * - under `prefers-reduced-motion: reduce` it never loops: a single static frame is drawn
 *   when the loop is started (and again on `redrawIfStatic`, e.g. after a resize)
 */
export interface AnimationLoop {
  start: () => void;
  stop: () => void;
  /** Draws one frame when the loop is not running (reduced motion, hidden tab); no-op otherwise. */
  redrawIfStatic: () => void;
}

export function createAnimationLoop(draw: () => void, maxFps = 30): AnimationLoop {
  const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
  const frameInterval = 1000 / maxFps;
  let rafId = 0;
  let lastFrame = 0;
  let started = false;

  const frame = (now: number) => {
    rafId = requestAnimationFrame(frame);
    // Small tolerance, since rAF timestamps jitter around the 33ms boundary
    if (now - lastFrame < frameInterval - 2) return;
    lastFrame = now;
    draw();
  };

  const cancel = () => {
    if (rafId) {
      cancelAnimationFrame(rafId);
      rafId = 0;
    }
  };

  const isAnimating = () => !reducedMotion.matches && !document.hidden;

  const sync = () => {
    if (!started) return;
    if (isAnimating()) {
      if (!rafId) rafId = requestAnimationFrame(frame);
    } else {
      cancel();
    }
  };

  const onMotionPreferenceChange = () => {
    sync();
    if (reducedMotion.matches) draw();
  };

  return {
    start() {
      if (started) return;
      started = true;
      document.addEventListener('visibilitychange', sync);
      reducedMotion.addEventListener('change', onMotionPreferenceChange);
      if (reducedMotion.matches) {
        draw();
      } else {
        sync();
      }
    },
    stop() {
      started = false;
      cancel();
      document.removeEventListener('visibilitychange', sync);
      reducedMotion.removeEventListener('change', onMotionPreferenceChange);
    },
    redrawIfStatic() {
      if (started && !rafId) draw();
    },
  };
}
