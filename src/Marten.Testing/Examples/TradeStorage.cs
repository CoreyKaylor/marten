﻿// SAMPLE: generated_trade_storage
using Marten;
using Marten.Linq;
using Marten.Schema;
using Marten.Services;
using Marten.Testing.Examples;
using Marten.Util;
using Npgsql;
using NpgsqlTypes;
using Remotion.Linq;
using System;
using System.Collections.Generic;

namespace Marten.GeneratedCode
{

    public class TradeStorage : IDocumentStorage, IBulkLoader<Trade>, IdAssignment<Trade>
    {

        private readonly Marten.Schema.Sequences.ISequence _sequence;

        public TradeStorage(Marten.Schema.Sequences.ISequence sequence)
        {
            _sequence = sequence;
        }


        public Type DocumentType => typeof(Trade);

        public NpgsqlCommand UpsertCommand(object document, string json)
        {
            return UpsertCommand((Trade)document, json);
        }


        public NpgsqlCommand LoaderCommand(object id)
        {
            return new NpgsqlCommand("select data from mt_doc_trade where id = :id").With("id", id);
        }


        public NpgsqlCommand DeleteCommandForId(object id)
        {
            return new NpgsqlCommand("delete from mt_doc_trade where id = :id").With("id", id);
        }


        public NpgsqlCommand DeleteCommandForEntity(object entity)
        {
            return DeleteCommandForId(((Trade)entity).Id);
        }


        public NpgsqlCommand LoadByArrayCommand<T>(T[] ids)
        {
            return new NpgsqlCommand("select data, id from mt_doc_trade where id = ANY(:ids)").With("ids", ids);
        }



        public NpgsqlCommand UpsertCommand(Trade document, string json)
        {
            return new NpgsqlCommand("mt_upsert_trade")
                .AsSproc()
                .With("id", document.Id)
                .WithJsonParameter("doc", json).With("arg_value", document.Value);
        }


        public object Assign(Trade document)
        {
            if (document.Id == 0) document.Id = _sequence.NextInt();
            return document.Id;
        }


        public object Retrieve(Trade document)
        {
            return document.Id;
        }


        public NpgsqlDbType IdType => NpgsqlDbType.Integer;

        public object Identity(object document)
        {
            return ((Marten.Testing.Examples.Trade)document).Id;
        }



        public void RegisterUpdate(UpdateBatch batch, object entity)
        {
            var document = (Marten.Testing.Examples.Trade)entity;
            batch.Sproc("mt_upsert_trade").Param(document.Id, NpgsqlDbType.Integer).JsonEntity(document).Param(document.Value, NpgsqlDbType.Double);
        }


        public void RegisterUpdate(UpdateBatch batch, object entity, string json)
        {
            var document = (Marten.Testing.Examples.Trade)entity;
            batch.Sproc("mt_upsert_trade").Param(document.Id, NpgsqlDbType.Integer).JsonBody(json).Param(document.Value, NpgsqlDbType.Double);
        }



        public void Load(ISerializer serializer, NpgsqlConnection conn, IEnumerable<Trade> documents)
        {
            using (var writer = conn.BeginBinaryImport("COPY mt_doc_trade(id, data, value) FROM STDIN BINARY"))
            {
                foreach (var x in documents)
                {
                    writer.StartRow();
                    writer.Write(x.Id, NpgsqlDbType.Integer);
                    writer.Write(serializer.ToJson(x), NpgsqlDbType.Jsonb);
                    writer.Write(x.Value, NpgsqlDbType.Double);
                }

            }

        }


    }




}
// ENDSAMPLE