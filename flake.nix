{
  description = "Example Go development environment for Zero to Nix";

  inputs = {
    nixpkgs.url = "https://flakehub.com/f/NixOS/nixpkgs/0";
  };

  outputs = {
    self,
    nixpkgs,
  }: let
    allSystems = [
      "x86_64-linux"
      "aarch64-linux"
      "x86_64-darwin"
      "aarch64-darwin"
    ];

    forAllSystems = f:
      nixpkgs.lib.genAttrs allSystems (
        system:
          f {
            pkgs = import nixpkgs {inherit system;};
          }
      );
  in {
    devShells = forAllSystems (
      {pkgs}: {
        default = pkgs.mkShell {
          packages = with pkgs; [
            (with dotnetCorePackages;
              combinePackages [
                sdk_9_0
                sdk_8_0
              ])
          ];
        };
      }
    );
  };
}
