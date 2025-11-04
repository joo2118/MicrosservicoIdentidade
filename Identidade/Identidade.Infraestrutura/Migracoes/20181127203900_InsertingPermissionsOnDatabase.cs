using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class InsertingPermissionsOnDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "user.management");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "user.registration");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "usergroup.management");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "usergroup.registration");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "831ba00e-e58c-440d-97a6-f7e2f8821694", "$2b$10$jgKX8IulAbktaIsuqG9Etu4yFxcLU7pwc4T3HuTUeQyddhYqcBgvm" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "UserGroupId" },
                values: new object[,]
                {
                    { "Movimentacao.Financeira", null, null },
                    { "opcoes", null, null },
                    { "operacoes", null, null },
                    { "participacao.acionaria", null, null },
                    { "perfil.controller", null, null },
                    { "perfis", null, null },
                    { "performanceattribution", null, null },
                    { "permissao", null, null },
                    { "permissao.carteiras", null, null },
                    { "pnl.agregado", null, null },
                    { "pnl.consolidado", null, null },
                    { "pnl.diario", null, null },
                    { "pnl.hipotetico", null, null },
                    { "moedas", null, null },
                    { "pnl.rentabilidade.cotas", null, null },
                    { "pnl.rentabilidade.tir", null, null },
                    { "pode.desconsiderar.alcada.calculos", null, null },
                    { "portal.administrativo", null, null },
                    { "posicoes.vista", null, null },
                    { "prazo.medio", null, null },
                    { "precificacao.parametrizavel", null, null },
                    { "produtosestruturados", null, null },
                    { "programa.hedge", null, null },
                    { "projecao.contas", null, null },
                    { "projecao.diferencas.creditos", null, null },
                    { "quantidadebloqueada", null, null },
                    { "recalculo.estatisticas", null, null },
                    { "registro.camara", null, null },
                    { "pnl.rentabilidade.timing.contribution", null, null },
                    { "registro.eventos", null, null },
                    { "modelos.pnl", null, null },
                    { "modelo.risco", null, null },
                    { "Historico.Vinculos", null, null },
                    { "importacao", null, null },
                    { "incluir.boleta.retroativa", null, null },
                    { "incluir.boleta.retroativa.periodo.fechamento.corrente", null, null },
                    { "incluir.custo", null, null },
                    { "indicadores.contratos", null, null },
                    { "indicadores.credito", null, null },
                    { "indicadores.empresas", null, null },
                    { "indicadores.performance", null, null },
                    { "instrucoes.pagamento", null, null },
                    { "juros.moratorios", null, null },
                    { "lancamentos.contabeis", null, null },
                    { "licencas.distribuicao", null, null },
                    { "modelos.fluxos.caixa", null, null },
                    { "licencas.importacao", null, null },
                    { "limites.alcada", null, null },
                    { "logs.resultados", null, null },
                    { "mapeamento.contas", null, null },
                    { "margem", null, null },
                    { "mascara.relatorios", null, null },
                    { "mercadorias.fisicas", null, null },
                    { "modelo.caixa", null, null },
                    { "modelo.cobertura.liquidez", null, null },
                    { "modelo.controller", null, null },
                    { "modelo.desinvestimento", null, null },
                    { "modelo.exposicao.credito", null, null },
                    { "modelo.liquidez", null, null },
                    { "modelo.margem", null, null },
                    { "limit.manager", null, null },
                    { "regra.dado.mercado", null, null },
                    { "relacoes.inter.company", null, null },
                    { "relatorio.boletas", null, null },
                    { "titulos", null, null },
                    { "tv01", null, null },
                    { "usuarios", null, null },
                    { "valida.workflow.dados.mercado", null, null },
                    { "validacao", null, null },
                    { "validacao.faturamento.pagamento", null, null },
                    { "validacao.faturamento.recebimento", null, null },
                    { "validar.lancamentos", null, null },
                    { "var.historico", null, null },
                    { "var.instrumento", null, null },
                    { "var.parametrico", null, null },
                    { "var.smc", null, null },
                    { "var.tve", null, null },
                    { "tipos.contrato", null, null },
                    { "Vinculo.Operacao", null, null },
                    { "visualizacao.cotas", null, null },
                    { "visualizacao.curvas", null, null },
                    { "visualizacao.limit.manager", null, null },
                    { "visualizacao.serieshistoricas", null, null },
                    { "visualizacao.superficies", null, null },
                    { "visualizacaobloqueios", null, null },
                    { "vp.analise.fluxo.caixa", null, null },
                    { "vp.gap.analysis", null, null },
                    { "vp.instrumento", null, null },
                    { "vp.stresstesting", null, null },
                    { "vp.vertice", null, null },
                    { "vv01", null, null },
                    { "webservice.integracao.analitico.tabular.acesso", null, null },
                    { "visualizacao.cenarios.temporais", null, null },
                    { "testes.impairment", null, null },
                    { "testes.consolidados", null, null },
                    { "teste.termo.critico", null, null },
                    { "relatorio.calcula", null, null },
                    { "relatorio.exporta", null, null },
                    { "relatorio.publica", null, null },
                    { "relatorio.regulatorio", null, null },
                    { "relatorio.relacoes.hedge", null, null },
                    { "relatorio.remove", null, null },
                    { "relatorio.salva", null, null },
                    { "relatorios", null, null },
                    { "remover.boleta.retroativa", null, null },
                    { "remover.boleta.retroativa.periodo.fechamento.corrente", null, null },
                    { "remover.composicao", null, null },
                    { "remover.custo", null, null },
                    { "reprovacao", null, null },
                    { "responsabilidade.controller", null, null },
                    { "risco.backtest", null, null },
                    { "risco.buscahedge", null, null },
                    { "rounding.rules", null, null },
                    { "salvar.analise.desinvestimento", null, null },
                    { "scripts", null, null },
                    { "series", null, null },
                    { "setores.economicos", null, null },
                    { "split.boleta.ou.liquidacao", null, null },
                    { "superficies", null, null },
                    { "swaps", null, null },
                    { "template.evento", null, null },
                    { "teste.analise.cenarios", null, null },
                    { "teste.analise.razao", null, null },
                    { "teste.regressao", null, null },
                    { "teste.regressao.prospectivo", null, null },
                    { "historico.reclassificacao.contabil", null, null },
                    { "xmapping", null, null },
                    { "historico.empresas", null, null },
                    { "historico.contratos", null, null },
                    { "calculo.liquidez", null, null },
                    { "calculo.relatorio.perdas", null, null },
                    { "calculo.relatorio.regulatorio", null, null },
                    { "calendarios", null, null },
                    { "campos.customizados", null, null },
                    { "carteiras", null, null },
                    { "cenarios.stress", null, null },
                    { "cenarios.temporais", null, null },
                    { "cfar", null, null },
                    { "classificacao.contabil", null, null },
                    { "classificadores", null, null },
                    { "classificar.banking.trading", null, null },
                    { "composicao.benchmark", null, null },
                    { "caixa", null, null },
                    { "composicao.capital.social", null, null },
                    { "conciliacao.pagamentos.recebimentos", null, null },
                    { "configuracao.aprecamento", null, null },
                    { "configuracao.campos.customizados", null, null },
                    { "configuracao.criterio.imposto", null, null },
                    { "configuracao.exposicao", null, null },
                    { "configuracao.extensibilidade", null, null },
                    { "configuracao.fluxo.caixa", null, null },
                    { "configuracao.valor.implicito", null, null },
                    { "configuracoes.mercadorias.fisicas", null, null },
                    { "contas", null, null },
                    { "contas.correntes", null, null },
                    { "contatos", null, null },
                    { "controle.custodia", null, null },
                    { "conciliacao.operacoes", null, null },
                    { "correcao.taxa", null, null },
                    { "cadastro.seguros", null, null },
                    { "cadastro.conciliacao.operacao", null, null },
                    { "adicionar.composicao", null, null },
                    { "agenda.financeira", null, null },
                    { "agendamento.tarefas", null, null },
                    { "agrupamentos.customizados", null, null },
                    { "agrupamentos.flexiveis", null, null },
                    { "ajuste.prudencial", null, null },
                    { "alterar.embarque.acordo.comercial", null, null },
                    { "alterar.preco.embarque.acordo.comercial", null, null },
                    { "alterar.tranche.acordo.comercial", null, null },
                    { "Analise.Comparativa.Perfis", null, null },
                    { "analise.exposicao.credito", null, null },
                    { "analise.fixacoes", null, null },
                    { "analise.precos.fixacoes", null, null },
                    { "cadastro.modelos.impairment", null, null },
                    { "aplicacaobloqueios", null, null },
                    { "aprovacao", null, null },
                    { "aprovacao.faturamento.pagamento", null, null },
                    { "aprovacao.faturamento.recebimento", null, null },
                    { "atualizacao.monetaria", null, null },
                    { "benchmarks", null, null },
                    { "bloqueios", null, null },
                    { "boletagem.alteracao.boletas.terceiros", null, null },
                    { "boletagem.alteracao.boletas.validadas", null, null },
                    { "boletagem.exportacao.operacoes", null, null },
                    { "boletagem.remocao.boletas.terceiros", null, null },
                    { "cadastra.configuracao.webservice", null, null },
                    { "cadastro.analitico.customizado", null, null },
                    { "cadastro.conciliacao.caixa", null, null },
                    { "aplicacaobloqueios.edicao", null, null },
                    { "criterio.aprecamento", null, null },
                    { "criterio.imposto", null, null },
                    { "curvas", null, null },
                    { "exportacao.itens.diretorio", null, null },
                    { "exportacao.lancamentos.contabeis", null, null },
                    { "exportacao.proventos", null, null },
                    { "exportacao.series", null, null },
                    { "exportacao.superficies", null, null },
                    { "exposicao", null, null },
                    { "exposicao.regulatoria", null, null },
                    { "exposicao.volumetrica", null, null },
                    { "expurgo.cenario.credito", null, null },
                    { "fatores.risco", null, null },
                    { "filtros", null, null },
                    { "fixacao.embarques", null, null },
                    { "fluxosfinanceiros", null, null },
                    { "exportacao.curvas", null, null },
                    { "fundos", null, null },
                    { "futuros", null, null },
                    { "geracao.cenario.credito", null, null },
                    { "gerar.lancamentos", null, null },
                    { "gestao.lancamentos", null, null },
                    { "grafico.contas", null, null },
                    { "graficos.historicos", null, null },
                    { "grupos.usuarios", null, null },
                    { "gv01", null, null },
                    { "habilita.customizacao.layout", null, null },
                    { "historico.atividades", null, null },
                    { "historico.campos", null, null },
                    { "historico.classificacao.posicao", null, null },
                    { "historico.composicoes", null, null },
                    { "fundos.abertos", null, null },
                    { "export.contabil", null, null },
                    { "expectativa.desinvestimento.quantidade", null, null },
                    { "Exercicio.Operacoes", null, null },
                    { "custo.medio", null, null },
                    { "custos.liquidacao", null, null },
                    { "datacaster", null, null },
                    { "desaprovacao", null, null },
                    { "desaprovacao.faturamento.pagamento", null, null },
                    { "desaprovacao.faturamento.recebimento", null, null },
                    { "desconsidera.workflow.salvamento.dados.mercado", null, null },
                    { "deslogar.usuario", null, null },
                    { "detalhes.analiticos", null, null },
                    { "dividas", null, null },
                    { "duration", null, null },
                    { "dv01", null, null },
                    { "edicao.arquivos.configuracao", null, null },
                    { "edicao.correlacao", null, null },
                    { "edicao.volatilidade", null, null },
                    { "edita.exportacao.ftp.controller", null, null },
                    { "edita.exportacao.local.controller", null, null },
                    { "edita.layouts.integracao.modelo.controller", null, null },
                    { "editar.boleta.retroativa", null, null },
                    { "editar.boleta.retroativa.periodo.fechamento.corrente", null, null },
                    { "editar.composicao", null, null },
                    { "editar.custo", null, null },
                    { "editar.lancamentos", null, null },
                    { "empresas", null, null },
                    { "empresas.capital.social", null, null },
                    { "empresas.detentoras", null, null },
                    { "exclusao.cenario", null, null },
                    { "execucao.housekeeping", null, null },
                    { "execucao.trilha.auditoria", null, null },
                    { "historico.desinvestimento", null, null },
                    { "acordoscomerciais", null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "acordoscomerciais");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "adicionar.composicao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "agenda.financeira");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "agendamento.tarefas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "agrupamentos.customizados");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "agrupamentos.flexiveis");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "ajuste.prudencial");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "alterar.embarque.acordo.comercial");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "alterar.preco.embarque.acordo.comercial");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "alterar.tranche.acordo.comercial");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "Analise.Comparativa.Perfis");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "analise.exposicao.credito");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "analise.fixacoes");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "analise.precos.fixacoes");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "aplicacaobloqueios");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "aplicacaobloqueios.edicao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "aprovacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "aprovacao.faturamento.pagamento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "aprovacao.faturamento.recebimento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "atualizacao.monetaria");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "benchmarks");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "bloqueios");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alteracao.boletas.terceiros");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alteracao.boletas.validadas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.exportacao.operacoes");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.remocao.boletas.terceiros");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "cadastra.configuracao.webservice");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "cadastro.analitico.customizado");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "cadastro.conciliacao.caixa");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "cadastro.conciliacao.operacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "cadastro.modelos.impairment");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "cadastro.seguros");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "caixa");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "calculo.liquidez");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "calculo.relatorio.perdas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "calculo.relatorio.regulatorio");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "calendarios");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "campos.customizados");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "carteiras");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "cenarios.stress");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "cenarios.temporais");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "cfar");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "classificacao.contabil");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "classificadores");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "classificar.banking.trading");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "composicao.benchmark");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "composicao.capital.social");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "conciliacao.operacoes");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "conciliacao.pagamentos.recebimentos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "configuracao.aprecamento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "configuracao.campos.customizados");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "configuracao.criterio.imposto");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "configuracao.exposicao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "configuracao.extensibilidade");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "configuracao.fluxo.caixa");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "configuracao.valor.implicito");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "configuracoes.mercadorias.fisicas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "contas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "contas.correntes");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "contatos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "controle.custodia");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "correcao.taxa");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "criterio.aprecamento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "criterio.imposto");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "curvas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "custo.medio");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "custos.liquidacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "datacaster");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "desaprovacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "desaprovacao.faturamento.pagamento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "desaprovacao.faturamento.recebimento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "desconsidera.workflow.salvamento.dados.mercado");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "deslogar.usuario");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "detalhes.analiticos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "dividas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "duration");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "dv01");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "edicao.arquivos.configuracao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "edicao.correlacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "edicao.volatilidade");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "edita.exportacao.ftp.controller");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "edita.exportacao.local.controller");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "edita.layouts.integracao.modelo.controller");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "editar.boleta.retroativa");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "editar.boleta.retroativa.periodo.fechamento.corrente");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "editar.composicao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "editar.custo");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "editar.lancamentos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "empresas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "empresas.capital.social");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "empresas.detentoras");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "exclusao.cenario");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "execucao.housekeeping");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "execucao.trilha.auditoria");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "Exercicio.Operacoes");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "expectativa.desinvestimento.quantidade");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "export.contabil");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "exportacao.curvas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "exportacao.itens.diretorio");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "exportacao.lancamentos.contabeis");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "exportacao.proventos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "exportacao.series");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "exportacao.superficies");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "exposicao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "exposicao.regulatoria");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "exposicao.volumetrica");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "expurgo.cenario.credito");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "fatores.risco");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "filtros");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "fixacao.embarques");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "fluxosfinanceiros");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "fundos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "fundos.abertos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "futuros");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "geracao.cenario.credito");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "gerar.lancamentos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "gestao.lancamentos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "grafico.contas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "graficos.historicos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "grupos.usuarios");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "gv01");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "habilita.customizacao.layout");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "historico.atividades");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "historico.campos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "historico.classificacao.posicao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "historico.composicoes");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "historico.contratos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "historico.desinvestimento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "historico.empresas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "historico.reclassificacao.contabil");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "Historico.Vinculos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "importacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "incluir.boleta.retroativa");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "incluir.boleta.retroativa.periodo.fechamento.corrente");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "incluir.custo");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "indicadores.contratos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "indicadores.credito");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "indicadores.empresas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "indicadores.performance");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "instrucoes.pagamento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "juros.moratorios");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "lancamentos.contabeis");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "licencas.distribuicao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "licencas.importacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "limit.manager");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "limites.alcada");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "logs.resultados");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "mapeamento.contas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "margem");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "mascara.relatorios");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "mercadorias.fisicas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "modelo.caixa");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "modelo.cobertura.liquidez");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "modelo.controller");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "modelo.desinvestimento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "modelo.exposicao.credito");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "modelo.liquidez");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "modelo.margem");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "modelo.risco");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "modelos.fluxos.caixa");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "modelos.pnl");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "moedas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "Movimentacao.Financeira");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "opcoes");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "operacoes");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "participacao.acionaria");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "perfil.controller");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "perfis");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "performanceattribution");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "permissao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "permissao.carteiras");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "pnl.agregado");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "pnl.consolidado");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "pnl.diario");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "pnl.hipotetico");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "pnl.rentabilidade.cotas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "pnl.rentabilidade.timing.contribution");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "pnl.rentabilidade.tir");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "pode.desconsiderar.alcada.calculos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "portal.administrativo");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "posicoes.vista");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "prazo.medio");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "precificacao.parametrizavel");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "produtosestruturados");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "programa.hedge");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "projecao.contas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "projecao.diferencas.creditos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "quantidadebloqueada");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "recalculo.estatisticas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "registro.camara");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "registro.eventos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "regra.dado.mercado");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "relacoes.inter.company");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "relatorio.boletas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "relatorio.calcula");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "relatorio.exporta");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "relatorio.publica");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "relatorio.regulatorio");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "relatorio.relacoes.hedge");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "relatorio.remove");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "relatorio.salva");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "relatorios");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "remover.boleta.retroativa");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "remover.boleta.retroativa.periodo.fechamento.corrente");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "remover.composicao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "remover.custo");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "reprovacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "responsabilidade.controller");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "risco.backtest");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "risco.buscahedge");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "rounding.rules");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "salvar.analise.desinvestimento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "scripts");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "series");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "setores.economicos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "split.boleta.ou.liquidacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "superficies");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "swaps");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "template.evento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "teste.analise.cenarios");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "teste.analise.razao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "teste.regressao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "teste.regressao.prospectivo");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "teste.termo.critico");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "testes.consolidados");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "testes.impairment");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "tipos.contrato");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "titulos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "tv01");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "usuarios");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "valida.workflow.dados.mercado");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "validacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "validacao.faturamento.pagamento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "validacao.faturamento.recebimento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "validar.lancamentos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "var.historico");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "var.instrumento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "var.parametrico");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "var.smc");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "var.tve");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "Vinculo.Operacao");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "visualizacao.cenarios.temporais");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "visualizacao.cotas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "visualizacao.curvas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "visualizacao.limit.manager");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "visualizacao.serieshistoricas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "visualizacao.superficies");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "visualizacaobloqueios");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "vp.analise.fluxo.caixa");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "vp.gap.analysis");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "vp.instrumento");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "vp.stresstesting");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "vp.vertice");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "vv01");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "webservice.integracao.analitico.tabular.acesso");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "xmapping");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "cc455035-c280-43a5-8715-44ff5e48a4eb", "$2b$10$I6J/6qgX3.3PNqVIY69R.OQKWGEyov.K85MFqTXIWmhmY1t2AzEOm" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "UserGroupId" },
                values: new object[,]
                {
                    { "user.registration", "User Registration", null },
                    { "usergroup.registration", "User Group Registration", null },
                    { "user.management", "User Management", null },
                    { "usergroup.management", "User Group Management", null }
                });
        }
    }
}
