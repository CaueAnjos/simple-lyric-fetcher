# simple-lyric-fetcher

slyricf is, as the name implies, a simple lyric fetcher. For short, it can get
lyrics from the internet and show them to you in your terminal. I am planning to
make it save these songs in a markdown format for future use to create
presentations. But for now it can't do this 🥲.

Currently, it only accepts songs from `letras.mus.br`. Planning to add more
providers soon 🥶!

## Installation 📦

You can install with **dotnet** or **Nix**.

1. **dotnet**

```bash
dotnet tool install -g slyricf
```

1. **Nix**

```bash
nix profile add github:CaueAnjos/simple-lyric-fetcher
```

With **Nix** you have the option to just run this program with:

```bash
nix run github:CaueAnjos/simple-lyric-fetcher
```

## Exploring 🧭

type `slyricf -h` on your terminal and try some urls from `letras.mus.br`!

## Unistalling

1. **dotnet**

```bash
dotnet tool uninstall -g slyricf
```

1. **Nix**

```bash
nix profile remove github:CaueAnjos/simple-lyric-fetcher
```
