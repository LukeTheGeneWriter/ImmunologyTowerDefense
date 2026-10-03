#!/usr/bin/env python3
"""Zero-dependency local static file server for testing the WebGL build.

Linux/macOS counterpart of serve_webgl.ps1. Needed for two reasons:
(1) file:// URLs can't load Unity's WebGL loader (it uses fetch(), which
browsers block under file://), and (2) Unity's WebGL output ships
gzip-compressed files that need a Content-Encoding header to be served
correctly -- `python -m http.server` doesn't set it, this does.

Usage: python3 tools/serve_webgl.py [--root DIR] [--port 8000]
"""
import argparse
import http.server
import os
import sys

MIME = {
    ".html": "text/html",
    ".js": "application/javascript",
    ".css": "text/css",
    ".wasm": "application/wasm",
    ".png": "image/png",
    ".ico": "image/x-icon",
    ".json": "application/json",
    ".data": "application/octet-stream",
}
ENCODINGS = {".gz": "gzip", ".br": "br"}


class Handler(http.server.SimpleHTTPRequestHandler):
    def guess_type(self, path):
        base, ext = os.path.splitext(path)
        if ext in ENCODINGS:
            ext = os.path.splitext(base)[1]
        return MIME.get(ext, "application/octet-stream")

    def end_headers(self):
        ext = os.path.splitext(self.path.split("?", 1)[0])[1]
        if ext in ENCODINGS:
            self.send_header("Content-Encoding", ENCODINGS[ext])
        self.send_header("Cache-Control", "no-store")
        super().end_headers()


def main():
    repo = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--root", default=os.path.join(repo, "game", "Builds", "WebGL"))
    ap.add_argument("--port", type=int, default=8000)
    args = ap.parse_args()

    if not os.path.isdir(args.root):
        sys.exit(f"Build folder not found at {args.root}. Build WebGL first.")

    handler = lambda *a, **kw: Handler(*a, directory=args.root, **kw)
    with http.server.ThreadingHTTPServer(("localhost", args.port), handler) as srv:
        print(f"Serving {args.root} at http://localhost:{args.port}/  (Ctrl+C to stop)")
        try:
            srv.serve_forever()
        except KeyboardInterrupt:
            pass


if __name__ == "__main__":
    main()
