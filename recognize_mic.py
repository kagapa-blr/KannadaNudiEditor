# -*- coding: utf-8 -*-
import speech_recognition as sr
import sys
import io
import threading
import queue
import time

# Ensure UTF-8 for stdout/stderr
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')

command_queue = queue.Queue()
exit_flag = threading.Event()

def command_listener():
    """Reads stdin line by line and places commands in the queue."""
    while not exit_flag.is_set():
        try:
            line = sys.stdin.readline()
            if not line:
                break  # EOF reached
            command_queue.put(line.strip().lower())
        except Exception as e:
            print(f"[Command Error] {e}", file=sys.stderr, flush=True)
            break
    exit_flag.set()

def speech_loop(language='kn-IN'):
    recognizer = sr.Recognizer()
    mic = sr.Microphone()
    listening = False

    print("READY", file=sys.stderr, flush=True)

    with mic as source:
        recognizer.adjust_for_ambient_noise(source)

        while not exit_flag.is_set():
            try:
                # Non-blocking check for command
                while not command_queue.empty():
                    cmd = command_queue.get()
                    if cmd == 'start':
                        listening = True
                        print("Listening started.", file=sys.stderr, flush=True)
                    elif cmd == 'stop':
                        listening = False
                        print("Listening stopped.", file=sys.stderr, flush=True)
                    elif cmd == 'exit':
                        print("Exiting...", file=sys.stderr, flush=True)
                        exit_flag.set()
                        return
                    else:
                        print(f"Unknown command: {cmd}", file=sys.stderr, flush=True)

                if listening:
                    try:
                        print("Awaiting audio...", file=sys.stderr, flush=True)
                        audio = recognizer.listen(source, timeout=10)
                        text = recognizer.recognize_google(audio, language=language)
                        print(text, flush=True)
                    except sr.WaitTimeoutError:
                        continue
                    except sr.UnknownValueError:
                        print("Could not understand.", file=sys.stderr, flush=True)
                    except sr.RequestError as e:
                        print(f"STT API error: {e}", file=sys.stderr, flush=True)
                else:
                    time.sleep(0.1)

            except Exception as e:
                print(f"Main loop error: {e}", file=sys.stderr, flush=True)

if __name__ == "__main__":
    lang = sys.argv[1] if len(sys.argv) > 1 else 'kn-IN'

    listener_thread = threading.Thread(target=command_listener, daemon=True)
    listener_thread.start()

    speech_loop(lang)
