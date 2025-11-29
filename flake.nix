{
  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
  inputs.flake-utils.url = "github:numtide/flake-utils";

  outputs = {
    self,
    flake-utils,
    nixpkgs,
    ...
  }:
    flake-utils.lib.eachDefaultSystem (system: let
      pkgs = nixpkgs.legacyPackages.${system};
      dotnet-sdk = pkgs.dotnetCorePackages.sdk_9_0;
    in {
      devShells.default = pkgs.mkShell {
        packages = with pkgs; [
          dotnet-sdk
          csharpier
        ];
      };

      packages.default =
        pkgs.buildDotnetModule
        {
          pname = "slyricf";
          version = "0.0.0";
          src = ./.;
          projectFile = "src/slyricf/slyricf.csproj";
          inherit dotnet-sdk;
          nugetDeps = ./deps.json;
        };
    });
}
